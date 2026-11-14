using Microsoft.EntityFrameworkCore;
using ReMarket.Application.MuaBan;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Commerce;

public sealed partial class MuaBanService
{
    public async Task<HoiThoaiDto> MoChatDonAsync(Guid orderId, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (order, tin, _) = await KhoaDonAsync(orderId, actor, ct);
        var chat = await db.CuocTroChuyen.SingleOrDefaultAsync(x => x.TinDangId == tin.Id && x.NguoiMuaId == order.NguoiMuaId, ct);
        if (chat == null)
        {
            chat = new CuocTroChuyen { TinDangId = tin.Id, NguoiMuaId = order.NguoiMuaId, Loai = LoaiTroChuyen.TrucTiep, TieuDe = (await db.SanPham.SingleAsync(x => x.Id == tin.SanPhamId, ct)).Ten };
            db.Add(chat); db.AddRange(new ThanhVienTroChuyen { CuocTroChuyenId = chat.Id, NguoiDungId = order.NguoiMuaId },
                new ThanhVienTroChuyen { CuocTroChuyenId = chat.Id, NguoiDungId = order.NguoiBanId });
            await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct); return await ChatAsync(chat.Id, actor, null, ct);
    }
    private async Task<CuocTroChuyen> ChatCuaToiAsync(Guid id, Guid actor, bool locked, CancellationToken ct)
    {
        if (!await db.ThanhVienTroChuyen.AnyAsync(x => x.CuocTroChuyenId == id && x.NguoiDungId == actor, ct)) throw new LoiTaiKhoan(404, "Bạn không thuộc hội thoại này.");
        return locked
            ? await db.CuocTroChuyen.FromSqlInterpolated($"SELECT * FROM dbo.CuocTroChuyen WITH (UPDLOCK, ROWLOCK) WHERE Id={id}").SingleAsync(ct)
            : await db.CuocTroChuyen.AsNoTracking().SingleAsync(x => x.Id == id, ct);
    }
    public async Task<HoiThoaiDto> MoChatAsync(Guid buyer, Guid tinId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (tin, product) = await stock.TinAsync(tinId, ct);
        var old = await db.CuocTroChuyen.SingleOrDefaultAsync(x => x.TinDangId == tinId && x.NguoiMuaId == buyer, ct);
        if (old != null) { await tx.CommitAsync(ct); return await ChatAsync(old.Id, buyer, null, ct); }
        await catalog.ChiTietAsync(tinId, buyer, false, ct);
        if (tin.NguoiDangId == buyer) throw new LoiTaiKhoan(400, "Không thể tự mở chat cho tin của bạn.");
        var chat = new CuocTroChuyen { TinDangId = tinId, NguoiMuaId = buyer, Loai = LoaiTroChuyen.TrucTiep, TieuDe = product.Ten };
        db.Add(chat);
        db.AddRange(new ThanhVienTroChuyen { CuocTroChuyenId = chat.Id, NguoiDungId = buyer },
            new ThanhVienTroChuyen { CuocTroChuyenId = chat.Id, NguoiDungId = tin.NguoiDangId });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await ChatAsync(chat.Id, buyer, null, ct);
    }
    public async Task<List<HopThuMuaDto>> HopThuAsync(Guid actor, bool unread, int page, CancellationToken ct)
    {
        var memberships = db.ThanhVienTroChuyen.AsNoTracking().Where(x => x.NguoiDungId == actor);
        if (unread) memberships = memberships.Where(m => db.TinNhan.Any(t => t.CuocTroChuyenId == m.CuocTroChuyenId && t.SoThuTu > m.SoThuTuDaDoc && t.NguoiGuiId != actor));
        var rows = await memberships.Select(m => new {
            Chat = m.CuocTroChuyen, Doc = m.SoThuTuDaDoc,
            Last = db.TinNhan.Where(t => t.CuocTroChuyenId == m.CuocTroChuyenId).OrderByDescending(t => t.SoThuTu).Select(t => (long?)t.SoThuTu).FirstOrDefault()
        }).OrderByDescending(x => x.Last).ThenByDescending(x => x.Chat.NgayTao).ThenBy(x => x.Chat.Id).Skip((page - 1) * 20).Take(20).ToListAsync(ct);
        var result = new List<HopThuMuaDto>();
        foreach (var row in rows)
        {
            var partners = await db.ThanhVienTroChuyen.Where(x => x.CuocTroChuyenId == row.Chat.Id && x.NguoiDungId != actor).Select(x => x.NguoiDungId).ToListAsync(ct);
            var names = await db.HoSoCaNhan.Where(x => partners.Contains(x.NguoiDungId)).OrderBy(x => x.HoTen).Select(x => x.HoTen).ToListAsync(ct);
            var last = await db.TinNhan.Where(x => x.CuocTroChuyenId == row.Chat.Id).OrderByDescending(x => x.SoThuTu).FirstOrDefaultAsync(ct);
            var count = await db.TinNhan.CountAsync(x => x.CuocTroChuyenId == row.Chat.Id && x.SoThuTu > row.Doc && x.NguoiGuiId != actor, ct);
            string? picture = null;
            if (row.Chat.TinDangId != null)
            {
                var productId = await db.TinDang.Where(x => x.Id == row.Chat.TinDangId).Select(x => x.SanPhamId).SingleAsync(ct);
                if (await db.AnhSanPham.AnyAsync(x => x.SanPhamId == productId, ct)) picture = $"api/hoithoai/{row.Chat.Id}/sanpham/anh";
            }
            result.Add(new(row.Chat.Id, row.Chat.TieuDe, last?.NoiDung ?? (last?.Loai == LoaiTinNhan.HinhAnh ? "Hình ảnh" : null),
                last?.NgayTao ?? row.Chat.NgayTao, count, string.Join(", ", names), picture));
        }
        return result;
    }
    public async Task<HoiThoaiDto> ChatAsync(Guid id, Guid actor, long? before, CancellationToken ct)
    {
        var chat = await ChatCuaToiAsync(id, actor, false, ct);
        var partners = await db.ThanhVienTroChuyen.Where(x => x.CuocTroChuyenId == id && x.NguoiDungId != actor).Select(x => x.NguoiDungId).ToListAsync(ct);
        var names = await db.HoSoCaNhan.Where(x => partners.Contains(x.NguoiDungId)).OrderBy(x => x.HoTen).Select(x => x.HoTen).ToListAsync(ct);
        var messages = await db.TinNhan.AsNoTracking().Where(x => x.CuocTroChuyenId == id && (before == null || x.SoThuTu < before))
            .OrderByDescending(x => x.SoThuTu).Take(50).ToListAsync(ct);
        messages.Reverse();
        var ids = messages.Select(x => x.Id).ToList();
        var files = await db.TepTinNhan.AsNoTracking().Where(x => ids.Contains(x.TinNhanId)).ToListAsync(ct);
        var offerRows = await db.PhieuThuongLuong.AsNoTracking().Where(x => x.CuocTroChuyenId == id && (ids.Contains(x.TinNhanId) || x.TrangThai == "DaChapNhan"))
            .OrderBy(x => x.TinNhan.SoThuTu).Take(100).ToListAsync(ct);
        SanPhamMuaDto? product = chat.TinDangId == null ? null : await SanPhamDtoAsync(chat.TinDangId.Value, $"api/hoithoai/{id}/sanpham/anh", ct);
        if (messages.Count > 0 && before == null)
        {
            var seq = messages[^1].SoThuTu;
            await db.ThanhVienTroChuyen.Where(x => x.CuocTroChuyenId == id && x.NguoiDungId == actor && x.SoThuTuDaDoc < seq)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.SoThuTuDaDoc, seq).SetProperty(p => p.LanDocCuoi, Now), ct);
        }
        var sale = chat.TinDangId != null && await db.TinDang.AnyAsync(x => x.Id == chat.TinDangId && (x.LoaiTin == LoaiTinDang.MuaBan || x.LoaiTin == LoaiTinDang.CaHai), ct);
        return new(chat.Id, chat.TieuDe, partners.FirstOrDefault(), string.Join(", ", names), chat.NguoiMuaId, product,
            messages.Select(x => new TinNhanDto(x.Id, x.SoThuTu, x.NguoiGuiId, x.Loai.ToString(), x.NoiDung, x.NgayTao,
                files.Where(f => f.TinNhanId == x.Id).Select(f => new AnhChatDto(f.Id, $"api/hoithoai/{id}/anh/{f.Id}")).ToList(),x.PhienBanThoaThuanId)).ToList(),
            offerRows.Select(PhieuDto).ToList(), messages.Count > 0 && await db.TinNhan.AnyAsync(x => x.CuocTroChuyenId == id && x.SoThuTu < messages[0].SoThuTu, ct),
            product != null && chat.NguoiMuaId != null && product.TrangThaiTin == "DangHienThi" && sale);
    }
    public async Task<HoiThoaiDto> GuiTinAsync(Guid id, Guid actor, GuiTinRequest request, CancellationToken ct)
    {
        MaHopLe(request.MaPhiaClient);
        var text = request.NoiDung.Trim(); if (text.Length == 0) throw new LoiTaiKhoan(400, "Tin nhắn không được để trống.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await ChatCuaToiAsync(id, actor, true, ct);
        var old = await db.TinNhan.SingleOrDefaultAsync(x => x.CuocTroChuyenId == id && x.NguoiGuiId == actor && x.MaPhiaClient == request.MaPhiaClient, ct);
        if (old != null)
        {
            if (old.Loai != LoaiTinNhan.VanBan || old.NoiDung != text) throw new LoiTaiKhoan(409, "Mã tin nhắn đã dùng cho nội dung khác.");
        }
        else db.Add(new TinNhan { CuocTroChuyenId = id, NguoiGuiId = actor, MaPhiaClient = request.MaPhiaClient, Loai = LoaiTinNhan.VanBan, NoiDung = text, NgayTao = Now });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await ChatAsync(id, actor, null, ct);
    }
    public async Task<HoiThoaiDto> GuiAnhAsync(Guid id, Guid actor, Guid key, Stream input, CancellationToken ct)
    {
        MaHopLe(key); await ChatCuaToiAsync(id, actor, false, ct);
        var file = await images.LuuAsync(input, ct);
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await ChatCuaToiAsync(id, actor, true, ct);
            var old = await db.TinNhan.SingleOrDefaultAsync(x => x.CuocTroChuyenId == id && x.NguoiGuiId == actor && x.MaPhiaClient == key, ct);
            if (old != null)
            {
                if (old.Loai != LoaiTinNhan.HinhAnh) throw new LoiTaiKhoan(409, "Mã tin nhắn đã được sử dụng.");
                images.Xoa(file);
            }
            else
            {
                var message = new TinNhan { CuocTroChuyenId = id, NguoiGuiId = actor, MaPhiaClient = key, Loai = LoaiTinNhan.HinhAnh, NoiDung = "Hình ảnh", NgayTao = Now };
                db.Add(message); db.Add(new TepTinNhan { TinNhanId = message.Id, DuongDan = file, TenTep = "anh.jpg", LoaiMime = "image/jpeg", DungLuong = new FileInfo(images.DuongDan(file)).Length });
            }
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        }
        catch { images.Xoa(file); throw; }
        return await ChatAsync(id, actor, null, ct);
    }
    public async Task<string> AnhChatAsync(Guid id, Guid fileId, Guid actor, CancellationToken ct)
    {
        await ChatCuaToiAsync(id, actor, false, ct);
        var file = await db.TepTinNhan.Where(x => x.Id == fileId && x.TinNhan.CuocTroChuyenId == id).Select(x => x.DuongDan).SingleOrDefaultAsync(ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy ảnh trong hội thoại.");
        var path = images.DuongDan(file); if (!File.Exists(path)) throw new LoiTaiKhoan(404, "Ảnh không còn tồn tại."); return path;
    }
    public async Task<string> AnhMonChatAsync(Guid id, Guid actor, CancellationToken ct)
    {
        var chat = await ChatCuaToiAsync(id, actor, false, ct);
        var productId = await db.TinDang.Where(x => x.Id == chat.TinDangId).Select(x => (Guid?)x.SanPhamId).SingleOrDefaultAsync(ct)
            ?? throw new LoiTaiKhoan(404, "Hội thoại không có món đồ.");
        return await AnhSanPhamRiengAsync(productId, ct);
    }
    public async Task<HoiThoaiDto> TaoPhieuAsync(Guid id, Guid actor, TaoPhieuRequest request, CancellationToken ct)
    {
        MaHopLe(request.MaPhiaClient);
        var initial = await ChatCuaToiAsync(id, actor, false, ct);
        if (initial.TinDangId == null || initial.NguoiMuaId != actor) throw new LoiTaiKhoan(403, "Chỉ người mua tạo đề nghị giảm giá trong hội thoại mua bán.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (tin, _) = await stock.TinAsync(initial.TinDangId.Value, ct);
        await ChatCuaToiAsync(id, actor, true, ct);
        var old = await db.TinNhan.SingleOrDefaultAsync(x => x.CuocTroChuyenId == id && x.NguoiGuiId == actor && x.MaPhiaClient == request.MaPhiaClient, ct);
        if (old != null)
        {
            var previous = await db.PhieuThuongLuong.SingleOrDefaultAsync(x => x.TinNhanId == old.Id, ct);
            if (previous == null || previous.SoTienGiam != request.SoTienGiam) throw new LoiTaiKhoan(409, "Mã đề nghị đã dùng với nội dung khác.");
            await tx.CommitAsync(ct); return await ChatAsync(id, actor, null, ct);
        }
        await SanPhamBanDuocAsync(tin.Id, actor, ct);
        if (request.SoTienGiam <= 0 || request.SoTienGiam >= tin.GiaBan) throw new LoiTaiKhoan(400, "Số tiền giảm phải lớn hơn 0 và nhỏ hơn giá niêm yết.");
        await db.PhieuThuongLuong.Where(x => x.CuocTroChuyenId == id && x.TrangThai == "ChoPhanHoi").ExecuteUpdateAsync(x => x.SetProperty(p => p.TrangThai, "DaHuy"), ct);
        var message = new TinNhan { CuocTroChuyenId = id, NguoiGuiId = actor, Loai = LoaiTinNhan.DeNghiGia, MaPhiaClient = request.MaPhiaClient, NoiDung = "Đề nghị giảm giá", NgayTao = Now };
        db.Add(message); db.Add(new PhieuThuongLuong { CuocTroChuyenId = id, TinNhanId = message.Id, NguoiMuaId = actor,
            NguoiBanId = tin.NguoiDangId, GiaNiemYet = tin.GiaBan!.Value, SoTienGiam = request.SoTienGiam, NgayHetHan = Now.AddHours(24), NgayTao = Now });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await ChatAsync(id, actor, null, ct);
    }
    public async Task<HoiThoaiDto> PhanHoiPhieuAsync(Guid id, Guid couponId, Guid actor, PhanHoiPhieuRequest request, CancellationToken ct)
    {
        var initial = await ChatCuaToiAsync(id, actor, false, ct);
        if (initial.TinDangId == null) throw new LoiTaiKhoan(409, "Hội thoại không có tin mua bán.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var (tin, _) = await stock.TinAsync(initial.TinDangId.Value, ct);
        await ChatCuaToiAsync(id, actor, true, ct);
        var coupon = await db.PhieuThuongLuong.SingleOrDefaultAsync(x => x.Id == couponId && x.CuocTroChuyenId == id, ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy phiếu.");
        KhoaTonKho.PhienBan(coupon.PhienBan, request.PhienBan);
        if (coupon.TrangThai is not ("ChoPhanHoi" or "DaChapNhan") || coupon.NgayHetHan <= Now) throw new LoiTaiKhoan(409, "Phiếu đã xử lý hoặc hết hạn.");
        if (request.ChapNhan)
        {
            if (actor != coupon.NguoiBanId) throw new LoiTaiKhoan(403, "Chỉ người bán chấp nhận mức giảm.");
            if (coupon.TrangThai != "ChoPhanHoi" || tin.TrangThai != TrangThaiTinDang.DangHienThi || tin.NgayXoa != null || tin.GiaBan != coupon.GiaNiemYet)
                throw new LoiTaiKhoan(409, "Tin hoặc đề nghị đã thay đổi, hãy tạo đề nghị mới.");
            await db.PhieuThuongLuong.Where(x => x.CuocTroChuyenId == id && x.TrangThai == "DaChapNhan" && x.Id != couponId)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.TrangThai, "DaHuy"), ct);
            coupon.TrangThai = "DaChapNhan";
        }
        else
        {
            if (actor == coupon.NguoiMuaId) coupon.TrangThai = "DaHuy";
            else if (actor == coupon.NguoiBanId && coupon.TrangThai == "ChoPhanHoi") coupon.TrangThai = "TuChoi";
            else throw new LoiTaiKhoan(403, "Không thể từ chối phiếu này.");
        }
        db.Add(new TinNhan { CuocTroChuyenId = id, NguoiGuiId = actor, Loai = LoaiTinNhan.HeThong, MaPhiaClient = Guid.NewGuid(),
            NoiDung = coupon.TrangThai == "DaChapNhan" ? "Người bán đã đồng ý phiếu giảm giá." : "Phiếu giảm giá đã bị từ chối hoặc hủy.", NgayTao = Now });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await ChatAsync(id, actor, null, ct);
    }
}
