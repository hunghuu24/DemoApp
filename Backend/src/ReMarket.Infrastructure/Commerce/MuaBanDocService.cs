using Microsoft.EntityFrameworkCore;
using ReMarket.Application.MuaBan;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Commerce;

public sealed partial class MuaBanService
{
    public async Task<DonHangDto> DonAsync(Guid id, Guid actor, CancellationToken ct, bool expire = true)
    {
        var order = await db.DonHang.AsNoTracking().Include(x => x.GiaoDich).SingleOrDefaultAsync(x => x.Id == id && (x.NguoiMuaId == actor || x.NguoiBanId == actor), ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy đơn của bạn.");
        if (expire && order.TrangThai == TrangThaiDonHang.ChoThanhToan && order.HanThanhToan <= Now)
            return await HuyAsync(id, actor, null, "Hết hạn thanh toán mô phỏng", true, ct);
        var line = await db.ChiTietDonHang.AsNoTracking().SingleAsync(x => x.DonHangId == id, ct);
        var product = await SanPhamDtoAsync(line.TinDangId, $"api/donhang/{id}/anh", ct);
        product = product with { Ten = line.TenSanPham, GiaNiemYet = line.GiaNiemYet ?? line.DonGia + line.SoTienGiam };
        var people = await db.HoSoCaNhan.Where(x => x.NguoiDungId == order.NguoiMuaId || x.NguoiDungId == order.NguoiBanId).ToDictionaryAsync(x => x.NguoiDungId, x => x.HoTen, ct);
        var pay = await db.ThanhToan.AsNoTracking().SingleAsync(x => x.DonHangId == id, ct);
        var ship = await db.VanChuyenMoPhong.AsNoTracking().SingleOrDefaultAsync(x => x.DonHangId == id, ct);
        var received = await db.XacNhanGiaoNhan.Where(x => x.GiaoDichId == order.GiaoDichId && x.Loai == LoaiXacNhan.DaNhan).Select(x => (DateTimeOffset?)x.NgayXacNhan).FirstOrDefaultAsync(ct);
        var review = await db.DanhGia.AsNoTracking().SingleOrDefaultAsync(x => x.GiaoDichId == order.GiaoDichId && x.NguoiDanhGiaId == order.NguoiMuaId && x.NguoiDuocDanhGiaId == order.NguoiBanId, ct);
        var timeline = new List<MocDonDto> { new("DaDatHang", "Đã đặt hàng", order.NgayTao) };
        if (pay.NgayThanhToan != null || pay.TrangThai is TrangThaiThanhToan.ThanhCong or TrangThaiThanhToan.HoanTien)
            timeline.Add(new("DaThanhToan", "Đã thanh toán mô phỏng", pay.NgayThanhToan ?? pay.NgayTao));
        if (received != null) timeline.Add(new("DaNhan", "Người mua xác nhận nhận hàng", received));
        if (order.NgayHuy != null) timeline.Add(new("DaHuy", order.LyDoHuy ?? "Đã hủy", order.NgayHuy));
        var steps = ship == null ? [] : await db.LichSuVanChuyen.AsNoTracking().Where(x => x.VanChuyenMoPhongId == ship.Id)
            .OrderBy(x => x.ThoiDiem).ThenBy(x => x.SoThuTu).Select(x => new MocDonDto(x.TrangThai.ToString(), x.MoTa, x.ThoiDiem)).ToListAsync(ct);
        var isBuyer = actor == order.NguoiMuaId;
        var active = order.TrangThai is TrangThaiDonHang.DaThanhToan or TrangThaiDonHang.DangGiao;
        return new(order.Id, order.GiaoDich.Ma, Convert.ToBase64String(order.PhienBan), order.TrangThai.ToString(), order.NguoiMuaId, order.NguoiBanId,
            people[order.NguoiMuaId], people[order.NguoiBanId], order.NgayTao, order.HanThanhToan, order.LyDoHuy, product,
            line.GiaNiemYet ?? line.DonGia + line.SoTienGiam, line.SoTienGiam, order.TienHang, order.PhiVanChuyen, order.TongTien,
            order.TenNguoiNhan, order.SoDienThoaiNhan, order.DiaChiNhan,
            new(pay.TrangThai.ToString(), pay.LaMoPhong, pay.TrangThai == TrangThaiThanhToan.ThanhCong && pay.NgayGiaiNgan == null, pay.NgayThanhToan, pay.NgayGiaiNgan, pay.NgayHoanTien),
            ship == null ? null : new(ship.MaVanDon, ship.TrangThai.ToString(), ship.NgayGiaoDuKien, steps), timeline,
            review == null ? null : new(review.Id, review.SoSao, review.SoSaoSanPham, review.NoiDung, review.NgayTao),
            isBuyer && order.TrangThai == TrangThaiDonHang.ChoThanhToan,
            order.TrangThai is TrangThaiDonHang.ChoThanhToan or TrangThaiDonHang.DaThanhToan && (ship == null || ship.TrangThai == TrangThaiVanChuyen.ChoLayHang),
            isBuyer && order.TrangThai == TrangThaiDonHang.DangGiao && ship?.TrangThai == TrangThaiVanChuyen.DaGiao,
            isBuyer && order.TrangThai == TrangThaiDonHang.DaNhan && review == null,
            !isBuyer && active && ship != null ? BuocShip(ship.TrangThai).Select(x => x.ToString()).ToList() : []);
    }
    public async Task<TrangDonDto> DanhSachDonAsync(Guid actor, string role, string? state, int page, CancellationToken ct)
    {
        if (role is not ("mua" or "ban")) throw new LoiTaiKhoan(400, "Vai trò đơn hàng không hợp lệ.");
        // Release this user's expired checkouts before computing tab counts and returning rows.
        var expired = await db.DonHang.AsNoTracking().Where(x => (x.NguoiMuaId == actor || x.NguoiBanId == actor) && x.TrangThai == TrangThaiDonHang.ChoThanhToan && x.HanThanhToan <= Now).Select(x => x.Id).ToListAsync(ct);
        foreach (var id in expired) await HuyAsync(id, actor, null, "Hết hạn thanh toán mô phỏng", true, ct);
        var query = db.DonHang.AsNoTracking().Where(x => role == "mua" ? x.NguoiMuaId == actor : x.NguoiBanId == actor);
        if (!string.IsNullOrEmpty(state))
        {
            if (!Enum.TryParse<TrangThaiDonHang>(state, out var parsed) || !Enum.IsDefined(parsed)) throw new LoiTaiKhoan(400, "Trạng thái đơn không hợp lệ.");
            query = query.Where(x => x.TrangThai == parsed);
        }
        var count = await query.CountAsync(ct);
        var ids = await query.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.Id).Skip((page - 1) * 10).Take(10).Select(x => x.Id).ToListAsync(ct);
        var rows = new List<DonHangDto>(); foreach (var id in ids) rows.Add(await DonAsync(id, actor, ct, false));
        return new(rows, count, page, (int)Math.Ceiling(count / 10d));
    }
    public async Task<string> AnhDonAsync(Guid id, Guid actor, CancellationToken ct)
    {
        if (!await db.DonHang.AnyAsync(x => x.Id == id && (x.NguoiMuaId == actor || x.NguoiBanId == actor), ct)) throw new LoiTaiKhoan(404, "Không tìm thấy đơn.");
        var line = await db.ChiTietDonHang.SingleAsync(x => x.DonHangId == id, ct);
        return await AnhSanPhamRiengAsync(line.SanPhamId, ct);
    }
    private async Task<string> AnhSanPhamRiengAsync(Guid id, CancellationToken ct)
    {
        var file = await db.AnhSanPham.Where(x => x.SanPhamId == id).OrderByDescending(x => x.LaAnhBia).ThenBy(x => x.ThuTu).Select(x => x.DuongDan).FirstOrDefaultAsync(ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy ảnh.");
        var path = images.DuongDan(file);
        if (!File.Exists(path)) throw new LoiTaiKhoan(404, "Ảnh không còn tồn tại.");
        return path;
    }
    public async Task<DanhGiaMuaDto> DanhGiaAsync(Guid id, Guid actor, GuiDanhGiaRequest request, CancellationToken ct)
    {
        MaHopLe(request.MaYeuCau);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (order, _, _) = await KhoaDonAsync(id, actor, ct);
        if (actor != order.NguoiMuaId) throw new LoiTaiKhoan(403, "Chỉ người mua đánh giá sản phẩm và người bán của đơn.");
        if (order.TrangThai != TrangThaiDonHang.DaNhan || !await db.GiaoDich.AnyAsync(x => x.Id == order.GiaoDichId && x.TrangThai == TrangThaiGiaoDich.HoanTat, ct))
            throw new LoiTaiKhoan(409, "Chỉ đánh giá sau khi đã xác nhận hoàn tất.");
        var old = await db.DanhGia.SingleOrDefaultAsync(x => x.GiaoDichId == order.GiaoDichId && x.NguoiDanhGiaId == actor && x.NguoiDuocDanhGiaId == order.NguoiBanId, ct);
        if (old != null)
        {
            if (old.MaYeuCau != request.MaYeuCau || old.SoSao != request.SoSaoNguoiBan || old.SoSaoSanPham != request.SoSaoSanPham || old.NoiDung != request.NoiDung.Trim())
                throw new LoiTaiKhoan(409, "Bạn đã đánh giá đơn này.");
            return new(old.Id, old.SoSao, old.SoSaoSanPham, old.NoiDung, old.NgayTao);
        }
        if (request.NoiDung.Trim().Length < 5) throw new LoiTaiKhoan(400, "Nhận xét cần ít nhất 5 ký tự.");
        var review = new DanhGia { GiaoDichId = order.GiaoDichId, NguoiDanhGiaId = actor, NguoiDuocDanhGiaId = order.NguoiBanId,
            SoSao = request.SoSaoNguoiBan, SoSaoSanPham = request.SoSaoSanPham, NoiDung = request.NoiDung.Trim(), MaYeuCau = request.MaYeuCau };
        db.Add(review); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return new(review.Id, review.SoSao, review.SoSaoSanPham, review.NoiDung, review.NgayTao);
    }
    public async Task<HoTroDto> TaoHoTroAsync(Guid actor, TaoHoTroRequest request, CancellationToken ct)
    {
        MaHopLe(request.MaYeuCau);
        if (!Enum.TryParse<LoaiYeuCauHoTro>(request.Loai, out var kind) || !Enum.IsDefined(kind)) throw new LoiTaiKhoan(400, "Loại hỗ trợ không hợp lệ.");
        if (request.TieuDe.Trim().Length < 5 || request.NoiDung.Trim().Length < 10) throw new LoiTaiKhoan(400, "Vui lòng mô tả rõ tiêu đề và vấn đề cần hỗ trợ.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        // Lock the actor to serialize identical support submissions, including requests without an order.
        await db.NguoiDung.FromSqlInterpolated($"SELECT * FROM dbo.NguoiDung WITH (UPDLOCK, ROWLOCK) WHERE Id={actor}").SingleAsync(ct);
        if (request.DonHangId != null && !await db.DonHang.AnyAsync(x => x.Id == request.DonHangId && (x.NguoiMuaId == actor || x.NguoiBanId == actor), ct))
            throw new LoiTaiKhoan(404, "Không thể gửi hỗ trợ cho đơn của người khác.");
        var existing = await db.YeuCauHoTro.SingleOrDefaultAsync(x => x.NguoiGuiId == actor && x.MaYeuCau == request.MaYeuCau, ct);
        if (existing != null)
        {
            if (existing.DonHangId != request.DonHangId || existing.TieuDe != request.TieuDe.Trim() || existing.NoiDung != request.NoiDung.Trim() || existing.Loai != kind)
                throw new LoiTaiKhoan(409, "Mã gửi hỗ trợ đã dùng cho nội dung khác.");
            return await HoTroAsync(existing.Id, actor, false, ct);
        }
        var support = new YeuCauHoTro { NguoiGuiId = actor, DonHangId = request.DonHangId, Loai = kind, TieuDe = request.TieuDe.Trim(),
            NoiDung = request.NoiDung.Trim(), TrangThai = TrangThaiHoTro.Moi, Ma = "HT" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(), MaYeuCau = request.MaYeuCau };
        db.Add(support); db.Add(new LichSuHoTro { YeuCauHoTroId = support.Id, NguoiThucHienId = actor, TrangThai = TrangThaiHoTro.Moi, NoiDung = "Đã gửi yêu cầu hỗ trợ" });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await HoTroAsync(support.Id, actor, false, ct);
    }
    public async Task<HoTroDto> HoTroAsync(Guid id, Guid actor, bool admin, CancellationToken ct)
    {
        var item = await db.YeuCauHoTro.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && (admin || x.NguoiGuiId == actor), ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy yêu cầu của bạn.");
        var rows = await db.LichSuHoTro.AsNoTracking().Where(x => x.YeuCauHoTroId == id).OrderBy(x => x.SoThuTu).ToListAsync(ct);
        // Existing tickets can have replies written before the event-history migration.
        // Preserve that text without inventing an earlier processing/resolution state.
        var legacyReplies = await db.PhanHoiHoTro.AsNoTracking().Where(p => p.YeuCauHoTroId == id &&
            !db.LichSuHoTro.Any(h => h.YeuCauHoTroId == id && h.NguoiThucHienId == p.NguoiGuiId && h.NoiDung == p.NoiDung && h.NgayTao == p.NgayTao))
            .OrderBy(p => p.NgayTao).ThenBy(p => p.Id).ToListAsync(ct);
        var actors = rows.Select(x => x.NguoiThucHienId).Concat(legacyReplies.Select(x => x.NguoiGuiId)).ToList();
        var names = await db.HoSoCaNhan.Where(x => actors.Contains(x.NguoiDungId)).ToDictionaryAsync(x => x.NguoiDungId, x => x.HoTen, ct);
        var code = item.DonHangId == null ? null : await db.DonHang.Where(x => x.Id == item.DonHangId).Select(x => x.GiaoDich.Ma).SingleAsync(ct);
        var history = rows.Select(x => new MocHoTroDto(x.TrangThai.ToString(), x.NoiDung, names.GetValueOrDefault(x.NguoiThucHienId, "ReMarket"), x.NgayTao)).ToList();
        if (!rows.Any(x => x.TrangThai == TrangThaiHoTro.Moi && x.NoiDung == "Đã gửi yêu cầu hỗ trợ")) history.Insert(0,new("Moi", "Đã gửi yêu cầu hỗ trợ", "Người gửi", item.NgayTao));
        history.AddRange(legacyReplies.Select(x => new MocHoTroDto("PhanHoi",x.NoiDung,names.GetValueOrDefault(x.NguoiGuiId,"ReMarket"),x.NgayTao)));
        history = history.OrderBy(x => x.NgayTao).ToList();
        return new(item.Id, item.Ma, Convert.ToBase64String(item.PhienBan), item.NguoiGuiId, item.DonHangId, code, item.Loai.ToString(), item.TrangThai.ToString(), item.TieuDe, item.NoiDung, item.NgayTao, history);
    }
    public async Task<List<HoTroDto>> DanhSachHoTroAsync(Guid actor, bool admin, int page, CancellationToken ct)
    {
        var ids = await db.YeuCauHoTro.AsNoTracking().Where(x => admin || x.NguoiGuiId == actor).OrderByDescending(x => x.NgayTao).ThenBy(x => x.Id)
            .Skip((page - 1) * 10).Take(10).Select(x => x.Id).ToListAsync(ct);
        var list = new List<HoTroDto>(); foreach (var id in ids) list.Add(await HoTroAsync(id, actor, admin, ct));
        return list;
    }
    public async Task<HoTroDto> PhanHoiHoTroAsync(Guid id, Guid actor, bool admin, PhanHoiHoTroRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var item = await db.YeuCauHoTro.FromSqlInterpolated($"SELECT * FROM dbo.YeuCauHoTro WITH (UPDLOCK, ROWLOCK) WHERE Id={id}").SingleOrDefaultAsync(ct);
        if (item == null || (!admin && item.NguoiGuiId != actor)) throw new LoiTaiKhoan(404, "Không tìm thấy yêu cầu của bạn.");
        KhoaTonKho.PhienBan(item.PhienBan, request.PhienBan);
        if (item.TrangThai == TrangThaiHoTro.DaGiaiQuyet) throw new LoiTaiKhoan(409, "Yêu cầu đã giải quyết; hãy gửi yêu cầu mới nếu cần.");
        if (!admin && request.TrangThai != null) throw new LoiTaiKhoan(403, "Chỉ quản trị viên thay trạng thái hỗ trợ.");
        if (request.NoiDung.Trim().Length < 5) throw new LoiTaiKhoan(400, "Nội dung bổ sung cần ít nhất 5 ký tự.");
        if (request.TrangThai != null)
        {
            if (!Enum.TryParse<TrangThaiHoTro>(request.TrangThai, out var next) || next is not (TrangThaiHoTro.DangXuLy or TrangThaiHoTro.DaGiaiQuyet))
                throw new LoiTaiKhoan(400, "Trạng thái hỗ trợ không hợp lệ.");
            item.TrangThai = next;
        }
        db.Entry(item).Property(x => x.TrangThai).IsModified = true;
        var now = Now;
        db.Add(new PhanHoiHoTro { YeuCauHoTroId = id, NguoiGuiId = actor, NoiDung = request.NoiDung.Trim(), NgayTao = now });
        db.Add(new LichSuHoTro { YeuCauHoTroId = id, NguoiThucHienId = actor, TrangThai = item.TrangThai, NoiDung = request.NoiDung.Trim(), NgayTao = now });
        db.Add(new ThongBao { NguoiDungId = item.NguoiGuiId, Loai = LoaiThongBao.HeThong, TieuDe = "Cập nhật hỗ trợ " + item.Ma, NoiDung = "Yêu cầu hỗ trợ có cập nhật mới." });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await HoTroAsync(id, actor, admin, ct);
    }
}
