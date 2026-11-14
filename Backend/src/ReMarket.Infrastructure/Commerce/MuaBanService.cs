using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ReMarket.Application.MuaBan;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Catalog;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.Commerce;

public sealed partial class MuaBanService(ReMarketDbContext db, KhoaTonKho stock, KhoAnhSanPham images,
    TinDangService catalog, TimeProvider clock, IConfiguration config)
{
    private DateTimeOffset Now => clock.GetUtcNow();
    private long ShipFee => config.GetValue<long>("Commerce:ShippingFee", 30_000);
    private static void MaHopLe(Guid key) { if (key == Guid.Empty) throw new LoiTaiKhoan(400, "Thiếu mã yêu cầu hợp lệ."); }
    private static string Phone(string value)
    {
        var phone = value.Trim().Replace(" ", "").Replace("-", "");
        if (phone.StartsWith("+84", StringComparison.Ordinal)) phone = "0" + phone[3..];
        if (phone.Length != 10 || phone[0] != '0' || !phone.All(char.IsAsciiDigit)) throw new LoiTaiKhoan(400, "Số điện thoại Việt Nam cần 10 chữ số, bắt đầu bằng 0.");
        return phone;
    }
    private static string BamDon(TaoDonRequest r) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
        new { r.TinDangId, r.PhieuThuongLuongId, Ten = r.TenNguoiNhan.Trim(), Phone = Phone(r.SoDienThoai), DiaChi = r.DiaChi.Trim(), r.PhuongThuc }))));
    private async Task SanPhamBanDuocAsync(Guid listingId, Guid buyer, CancellationToken ct)
    {
        var tin = await catalog.ChiTietAsync(listingId, null, false, ct);
        if (tin.NguoiDangId == buyer) throw new LoiTaiKhoan(403, "Bạn không thể mua món đồ của mình.");
        if (tin.LoaiTin is not ("MuaBan" or "CaHai") || tin.GiaBan is null or <= 0) throw new LoiTaiKhoan(409, "Tin này không bán theo luồng mua hàng.");
    }
    private PhieuGiamDto PhieuDto(PhieuThuongLuong p) => new(p.Id, p.TinNhanId, p.NguoiMuaId, p.NguoiBanId, p.GiaNiemYet,
        p.SoTienGiam, p.GiaNiemYet - p.SoTienGiam, p.NgayHetHan <= Now && p.TrangThai is "ChoPhanHoi" or "DaChapNhan" ? "HetHan" : p.TrangThai,
        p.NgayHetHan, Convert.ToBase64String(p.PhienBan));
    private async Task<PhieuThuongLuong?> PhieuHopLeAsync(Guid? coupon, Guid tinId, Guid buyer, long price, CancellationToken ct)
    {
        if (coupon is null) return null;
        var p = await db.PhieuThuongLuong.Include(x => x.CuocTroChuyen).SingleOrDefaultAsync(x => x.Id == coupon, ct);
        if (p is null || p.NguoiMuaId != buyer || p.CuocTroChuyen.TinDangId != tinId) throw new LoiTaiKhoan(400, "Phiếu giảm giá không dành cho tài khoản hoặc món đồ này.");
        if (p.TrangThai != "DaChapNhan" || p.NgayHetHan <= Now || p.GiaNiemYet != price)
            throw new LoiTaiKhoan(409, "Phiếu chưa được đồng ý, đã dùng/hết hạn hoặc giá niêm yết đã thay đổi.");
        return p;
    }
    private async Task<SanPhamMuaDto> SanPhamDtoAsync(Guid tinId, string imageRoute, CancellationToken ct)
    {
        var tin = await db.TinDang.AsNoTracking().Include(x => x.SanPham).SingleAsync(x => x.Id == tinId, ct);
        var name = await db.HoSoCaNhan.Where(x => x.NguoiDungId == tin.NguoiDangId).Select(x => x.HoTen).SingleAsync(ct);
        var photo = await db.AnhSanPham.Where(x => x.SanPhamId == tin.SanPhamId).OrderByDescending(x => x.LaAnhBia).ThenBy(x => x.ThuTu).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
        return new(tin.Id, tin.SanPhamId, tin.SanPham.Ten, tin.GiaBan ?? tin.SanPham.GiaTriThamKhao,
            photo == null ? null : imageRoute, tin.NguoiDangId, name, tin.SanPham.TinhTrang.ToString(), tin.TrangThai.ToString());
    }
    public async Task<BaoGiaDto> BaoGiaAsync(Guid buyer, BaoGiaRequest request, CancellationToken ct)
    {
        await GiaiPhongHetHanTheoTinAsync(request.TinDangId, ct);
        await SanPhamBanDuocAsync(request.TinDangId, buyer, ct);
        var tin = await db.TinDang.AsNoTracking().SingleAsync(x => x.Id == request.TinDangId, ct);
        if (await db.GiuChoSanPham.AnyAsync(x => x.SanPhamId == tin.SanPhamId && !x.DaGiaiPhong, ct)) throw new LoiTaiKhoan(409, "Món đồ đang được giữ cho giao dịch khác.");
        var coupons = await db.PhieuThuongLuong.AsNoTracking().Where(x => x.NguoiMuaId == buyer && x.CuocTroChuyen.TinDangId == tin.Id &&
            x.TrangThai == "DaChapNhan" && x.NgayHetHan > Now && x.GiaNiemYet == tin.GiaBan).ToListAsync(ct);
        var p = await PhieuHopLeAsync(request.PhieuThuongLuongId, tin.Id, buyer, tin.GiaBan!.Value, ct);
        var photoId = await db.AnhSanPham.Where(x => x.SanPhamId == tin.SanPhamId).OrderByDescending(x => x.LaAnhBia).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
        var product = await SanPhamDtoAsync(tin.Id, $"api/tindang/{tin.Id}/anh/{photoId}", ct);
        var reduction = p?.SoTienGiam ?? 0;
        return new(product, Convert.ToBase64String(tin.PhienBan), tin.GiaBan.Value, reduction, tin.GiaBan.Value - reduction,
            ShipFee, checked(tin.GiaBan.Value - reduction + ShipFee), p?.Id, coupons.Select(PhieuDto).ToList());
    }
    public async Task<DonHangDto> TaoDonAsync(Guid buyer, TaoDonRequest request, CancellationToken ct)
    {
        MaHopLe(request.MaYeuCau);
        if (request.PhuongThuc != "MoPhong") throw new LoiTaiKhoan(400, "Chỉ có thanh toán mô phỏng, không trừ tiền thật.");
        if (request.TenNguoiNhan.Trim().Length < 2 || request.DiaChi.Trim().Length < 10) throw new LoiTaiKhoan(400, "Vui lòng nhập đầy đủ thông tin giao hàng.");
        var hash = BamDon(request);
        var replay = await db.DonHang.AsNoTracking().SingleOrDefaultAsync(x => x.NguoiMuaId == buyer && x.MaYeuCau == request.MaYeuCau, ct);
        if (replay != null)
        {
            if (replay.BamYeuCau != hash) throw new LoiTaiKhoan(409, "Mã yêu cầu đã dùng cho nội dung đặt hàng khác.");
            return await DonAsync(replay.Id, buyer, ct);
        }
        await GiaiPhongHetHanTheoTinAsync(request.TinDangId, ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (tin, product) = await stock.TinAsync(request.TinDangId, ct);
        KhoaTonKho.PhienBan(tin.PhienBan, request.PhienBanTin);
        await SanPhamBanDuocAsync(tin.Id, buyer, ct);
        if (await db.GiuChoSanPham.AnyAsync(x => x.SanPhamId == product.Id && !x.DaGiaiPhong, ct)) throw new LoiTaiKhoan(409, "Món đồ vừa được giữ cho giao dịch khác.");
        var coupon = await PhieuHopLeAsync(request.PhieuThuongLuongId, tin.Id, buyer, tin.GiaBan!.Value, ct);
        var now = Now;
        var gd = new GiaoDich { Ma = "RM" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(), Loai = LoaiGiaoDich.MuaBan, TrangThai = TrangThaiGiaoDich.KhoiTao, NgayTao = now };
        var amount = tin.GiaBan.Value - (coupon?.SoTienGiam ?? 0);
        var don = new DonHang { GiaoDichId = gd.Id, NguoiMuaId = buyer, NguoiBanId = tin.NguoiDangId, NgayTao = now,
            TenNguoiNhan = request.TenNguoiNhan.Trim(), SoDienThoaiNhan = Phone(request.SoDienThoai), DiaChiNhan = request.DiaChi.Trim(),
            TienHang = amount, PhiVanChuyen = ShipFee, TongTien = checked(amount + ShipFee), TrangThai = TrangThaiDonHang.ChoThanhToan,
            HanThanhToan = now.AddMinutes(15), MaYeuCau = request.MaYeuCau, BamYeuCau = hash, PhieuThuongLuongId = coupon?.Id };
        db.Add(gd);
        db.AddRange(new ThanhVienGiaoDich { GiaoDichId = gd.Id, NguoiDungId = buyer, VaiTroHienThi = "NguoiMua" },
            new ThanhVienGiaoDich { GiaoDichId = gd.Id, NguoiDungId = tin.NguoiDangId, VaiTroHienThi = "NguoiBan" });
        db.Add(don);
        db.Add(new ChiTietDonHang { DonHangId = don.Id, TinDangId = tin.Id, SanPhamId = product.Id, TenSanPham = product.Ten,
            GiaNiemYet = tin.GiaBan, SoTienGiam = coupon?.SoTienGiam ?? 0, DonGia = amount, SoLuong = 1 });
        db.Add(new ThanhToan { DonHangId = don.Id, SoTien = don.TongTien, LaMoPhong = true, TrangThai = TrangThaiThanhToan.ChoXuLy });
        var ship = new VanChuyenMoPhong { DonHangId = don.Id, MaVanDon = "SIM" + don.Id.ToString("N")[..16].ToUpperInvariant(),
            TrangThai = TrangThaiVanChuyen.ChoLayHang, NgayGiaoDuKien = now.AddDays(3) };
        db.Add(ship); db.Add(new LichSuVanChuyen { VanChuyenMoPhongId = ship.Id, TrangThai = ship.TrangThai, MoTa = "Chờ thanh toán và bàn giao mô phỏng", ThoiDiem = now });
        db.Add(new GiuChoSanPham { SanPhamId = product.Id, GiaoDichId = gd.Id, NgayHetHan = don.HanThanhToan.Value });
        if (coupon != null) coupon.TrangThai = "DaSuDung";
        tin.TrangThai = TrangThaiTinDang.DangGiuCho; tin.NgayCapNhat = now;
        ThongBao(don, "Đơn mới", "Đơn hàng đã tạo, chờ thanh toán mô phỏng.");
        try { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        { throw new LoiTaiKhoan(409, "Yêu cầu hoặc món đồ vừa được xử lý. Hãy tải lại đơn hàng."); }
        return await DonAsync(don.Id, buyer, ct);
    }
    private void ThongBao(DonHang order, string title, string text)
    {
        foreach (var id in new[] { order.NguoiMuaId, order.NguoiBanId }) db.Add(new ThongBao {
            NguoiDungId = id, GiaoDichId = order.GiaoDichId, Loai = LoaiThongBao.DonHang, TieuDe = title, NoiDung = text });
    }
    private async Task<(DonHang Don, TinDang Tin, ChiTietDonHang Dong)> KhoaDonAsync(Guid id, Guid? actor, CancellationToken ct)
    {
        var line = await db.ChiTietDonHang.AsNoTracking().SingleOrDefaultAsync(x => x.DonHangId == id, ct) ?? throw new LoiTaiKhoan(404, "Không tìm thấy đơn hàng.");
        var (tin, _) = await stock.TinAsync(line.TinDangId, ct);
        var don = await db.DonHang.FromSqlInterpolated($"SELECT * FROM dbo.DonHang WITH (UPDLOCK, ROWLOCK) WHERE Id={id}").SingleAsync(ct);
        if (actor != null && actor != don.NguoiMuaId && actor != don.NguoiBanId) throw new LoiTaiKhoan(404, "Đơn không thuộc tài khoản của bạn.");
        return (don, tin, line);
    }
    private async Task GiaiPhongHetHanTheoTinAsync(Guid id, CancellationToken ct)
    {
        var orders = await db.DonHang.AsNoTracking().Where(x => x.TrangThai == TrangThaiDonHang.ChoThanhToan && x.HanThanhToan <= Now &&
            db.ChiTietDonHang.Any(l => l.DonHangId == x.Id && l.TinDangId == id)).Select(x => x.Id).ToListAsync(ct);
        foreach (var order in orders) await HuyAsync(order, null, null, "Hết hạn thanh toán mô phỏng", true, ct);
    }
    public async Task HetHanAsync(CancellationToken ct)
    {
        var ids = await db.DonHang.AsNoTracking().Where(x => x.TrangThai == TrangThaiDonHang.ChoThanhToan && x.HanThanhToan <= Now)
            .OrderBy(x => x.HanThanhToan).Take(20).Select(x => x.Id).ToListAsync(ct);
        foreach (var id in ids) await HuyAsync(id, null, null, "Hết hạn thanh toán mô phỏng", true, ct);
    }
    public async Task<DonHangDto> ThanhToanAsync(Guid id, Guid actor, ThanhToanRequest request, CancellationToken ct)
    {
        MaHopLe(request.MaYeuCau);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (don, _, _) = await KhoaDonAsync(id, actor, ct);
        if (actor != don.NguoiMuaId) throw new LoiTaiKhoan(403, "Chỉ người mua được thanh toán.");
        var payment = await db.ThanhToan.SingleAsync(x => x.DonHangId == id, ct);
        if (payment.MaYeuCauCuoi == request.MaYeuCau)
        {
            if (payment.KetQuaCuoi != request.ThanhCong) throw new LoiTaiKhoan(409, "Mã thanh toán đã dùng với kết quả khác.");
            await tx.CommitAsync(ct); return await DonAsync(id, actor, ct);
        }
        KhoaTonKho.PhienBan(don.PhienBan, request.PhienBan);
        if (don.TrangThai != TrangThaiDonHang.ChoThanhToan) throw new LoiTaiKhoan(409, "Đơn không còn chờ thanh toán.");
        if (don.HanThanhToan <= Now)
        {
            await tx.RollbackAsync(ct); await HuyAsync(id, actor, null, "Hết hạn thanh toán mô phỏng", true, ct);
            throw new LoiTaiKhoan(409, "Đơn đã hết hạn thanh toán và món đồ được giải phóng.");
        }
        payment.MaYeuCauCuoi = request.MaYeuCau; payment.KetQuaCuoi = request.ThanhCong;
        payment.TrangThai = request.ThanhCong ? TrangThaiThanhToan.ThanhCong : TrangThaiThanhToan.ThatBai;
        payment.LaMoPhong = true; payment.MaThamChieu = "SIM-" + request.MaYeuCau.ToString("N");
        if (request.ThanhCong)
        {
            payment.NgayThanhToan = Now; don.TrangThai = TrangThaiDonHang.DaThanhToan;
            await db.SaveChangesAsync(ct);
            var gd = await db.GiaoDich.SingleAsync(x => x.Id == don.GiaoDichId, ct); gd.TrangThai = TrangThaiGiaoDich.DangThucHien;
        }
        else db.Entry(don).Property(x => x.TrangThai).IsModified = true;
        ThongBao(don, request.ThanhCong ? "Đã thanh toán mô phỏng" : "Thanh toán mô phỏng thất bại", "Không trừ tiền thật. Trạng thái đã được lưu trong đơn.");
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await DonAsync(id, actor, ct);
    }
    public async Task<DonHangDto> HuyAsync(Guid id, Guid? actor, string? version, string? reason, bool expired, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (don, tin, _) = await KhoaDonAsync(id, actor, ct);
        if (don.TrangThai == TrangThaiDonHang.DaHuy || (expired && (don.TrangThai != TrangThaiDonHang.ChoThanhToan || don.HanThanhToan > Now || don.HanThanhToan == null)))
        { await tx.CommitAsync(ct); return await DonAsync(id, actor ?? don.NguoiMuaId, ct, false); }
        if (!expired)
        {
            KhoaTonKho.PhienBan(don.PhienBan, version ?? "");
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5) throw new LoiTaiKhoan(400, "Cần lý do hủy ít nhất 5 ký tự.");
        }
        var ship = await db.VanChuyenMoPhong.SingleOrDefaultAsync(x => x.DonHangId == id, ct);
        if (don.TrangThai is not (TrangThaiDonHang.ChoThanhToan or TrangThaiDonHang.DaThanhToan) || (ship != null && ship.TrangThai != TrangThaiVanChuyen.ChoLayHang))
            throw new LoiTaiKhoan(409, "Đã bàn giao vận chuyển hoặc hoàn tất; vui lòng gửi yêu cầu hỗ trợ.");
        don.TrangThai = TrangThaiDonHang.DaHuy; don.NgayHuy = Now; don.LyDoHuy = reason?.Trim();
        var payment = await db.ThanhToan.SingleAsync(x => x.DonHangId == id, ct);
        if (payment.TrangThai == TrangThaiThanhToan.ThanhCong) { payment.TrangThai = TrangThaiThanhToan.HoanTien; payment.NgayHoanTien = Now; }
        else payment.TrangThai = TrangThaiThanhToan.ThatBai;
        var gd = await db.GiaoDich.SingleAsync(x => x.Id == don.GiaoDichId, ct); gd.TrangThai = TrangThaiGiaoDich.DaHuy;
        await db.GiuChoSanPham.Where(x => x.GiaoDichId == don.GiaoDichId && !x.DaGiaiPhong).ExecuteUpdateAsync(x => x.SetProperty(p => p.DaGiaiPhong, true), ct);
        if (tin.NgayXoa == null && tin.TrangThai == TrangThaiTinDang.DangGiuCho) { tin.TrangThai = TrangThaiTinDang.DangHienThi; tin.NgayCapNhat = Now; }
        ThongBao(don, "Đơn đã hủy", reason ?? "Đã hủy đơn và giải phóng món đồ.");
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await DonAsync(id, actor ?? don.NguoiMuaId, ct, false);
    }
    public async Task<DonHangDto> ChuyenShipAsync(Guid id, Guid actor, ChuyenShipRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (don, _, line) = await KhoaDonAsync(id, actor, ct);
        if (actor != don.NguoiBanId) throw new LoiTaiKhoan(403, "Chỉ người bán điều khiển vận chuyển mô phỏng.");
        KhoaTonKho.PhienBan(don.PhienBan, request.PhienBan);
        if (don.TrangThai is not (TrangThaiDonHang.DaThanhToan or TrangThaiDonHang.DangGiao)) throw new LoiTaiKhoan(409, "Đơn cần được thanh toán trước khi vận chuyển.");
        var ship = await db.VanChuyenMoPhong.SingleAsync(x => x.DonHangId == id, ct);
        if (!Enum.TryParse<TrangThaiVanChuyen>(request.TrangThai, out var next) || !BuocShip(ship.TrangThai).Contains(next)) throw new LoiTaiKhoan(409, "Không thể bỏ qua hoặc lùi bước vận chuyển.");
        ship.TrangThai = next; don.TrangThai = TrangThaiDonHang.DangGiao;
        db.Entry(don).Property(x => x.TrangThai).IsModified = true;
        db.Add(new LichSuVanChuyen { VanChuyenMoPhongId = ship.Id, TrangThai = next, ThoiDiem = Now, MoTa = "Vận chuyển mô phỏng: " + next });
        if (next == TrangThaiVanChuyen.DaLayHang) db.Add(new XacNhanGiaoNhan { GiaoDichId = don.GiaoDichId, NguoiXacNhanId = don.NguoiBanId,
            NguoiDoiTacId = don.NguoiMuaId, SanPhamId = line.SanPhamId, Loai = LoaiXacNhan.DaGiao, NgayXacNhan = Now });
        ThongBao(don, "Cập nhật vận chuyển", "Trạng thái mô phỏng: " + next);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await DonAsync(id, actor, ct);
    }
    private static List<TrangThaiVanChuyen> BuocShip(TrangThaiVanChuyen state) => state switch {
        TrangThaiVanChuyen.ChoLayHang => [TrangThaiVanChuyen.DaLayHang],
        TrangThaiVanChuyen.DaLayHang => [TrangThaiVanChuyen.DangVanChuyen, TrangThaiVanChuyen.GiaoThatBai],
        TrangThaiVanChuyen.DangVanChuyen => [TrangThaiVanChuyen.DaGiao, TrangThaiVanChuyen.GiaoThatBai],
        TrangThaiVanChuyen.GiaoThatBai => [TrangThaiVanChuyen.DangVanChuyen], _ => [] };
    public async Task<DonHangDto> NhanHangAsync(Guid id, Guid actor, string version, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (don, tin, line) = await KhoaDonAsync(id, actor, ct);
        if (actor != don.NguoiMuaId) throw new LoiTaiKhoan(403, "Chỉ người mua xác nhận đã nhận hàng.");
        if (don.TrangThai == TrangThaiDonHang.DaNhan) { await tx.CommitAsync(ct); return await DonAsync(id, actor, ct); }
        KhoaTonKho.PhienBan(don.PhienBan, version);
        var ship = await db.VanChuyenMoPhong.SingleAsync(x => x.DonHangId == id, ct);
        var payment = await db.ThanhToan.SingleAsync(x => x.DonHangId == id, ct);
        if (don.TrangThai != TrangThaiDonHang.DangGiao || ship.TrangThai != TrangThaiVanChuyen.DaGiao || payment.TrangThai != TrangThaiThanhToan.ThanhCong)
            throw new LoiTaiKhoan(409, "Chỉ xác nhận khi đã thanh toán và vận chuyển báo đã giao.");
        don.TrangThai = TrangThaiDonHang.DaNhan; tin.TrangThai = TrangThaiTinDang.DaBan; tin.NgayCapNhat = Now; payment.NgayGiaiNgan = Now;
        db.Add(new XacNhanGiaoNhan { GiaoDichId = don.GiaoDichId, NguoiXacNhanId = actor, NguoiDoiTacId = don.NguoiBanId,
            SanPhamId = line.SanPhamId, Loai = LoaiXacNhan.DaNhan, NgayXacNhan = Now });
        await db.GiuChoSanPham.Where(x => x.GiaoDichId == don.GiaoDichId && !x.DaGiaiPhong).ExecuteUpdateAsync(x => x.SetProperty(p => p.DaGiaiPhong, true), ct);
        await db.SaveChangesAsync(ct);
        var gd = await db.GiaoDich.SingleAsync(x => x.Id == don.GiaoDichId, ct); gd.TrangThai = TrangThaiGiaoDich.HoanTat; gd.NgayHoanTat = Now;
        ThongBao(don, "Đã hoàn tất mua bán", "Người mua đã nhận hàng; khoản giữ mô phỏng được giải ngân, không có chuyển tiền thật.");
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await DonAsync(id, actor, ct);
    }
}
