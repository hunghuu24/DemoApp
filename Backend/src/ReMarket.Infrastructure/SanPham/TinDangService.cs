using System.Data;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SanPham;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.Catalog;

public sealed class TinDangService(ReMarketDbContext db, KhoAnhSanPham images, ReMarket.Infrastructure.Commerce.KhoaTonKho stock)
{
    private IQueryable<TinDang> CongKhai => db.TinDang.AsNoTracking().Where(t => t.NgayXoa == null &&
        t.TrangThai == TrangThaiTinDang.DangHienThi && t.SanPham.ChuSoHuu.TrangThai == TrangThaiNguoiDung.HoatDong);
    private IQueryable<TinDang> MuaBan(IQueryable<TinDang> query) => query.Where(t => t.LoaiTin == LoaiTinDang.MuaBan || t.LoaiTin == LoaiTinDang.CaHai);
    private async Task<List<DanhMuc>> DanhMucHienAsync(CancellationToken ct)
    {
        var all = await db.DanhMuc.AsNoTracking().Where(x => x.DangHienThi).OrderBy(x => x.ThuTu).ThenBy(x => x.Id).ToListAsync(ct);
        var result = new List<DanhMuc>();
        var ids = new HashSet<Guid>();
        // Hidden parents hide their descendants; cycles are never traversed indefinitely.
        while (true)
        {
            var next = all.Where(x => !ids.Contains(x.Id) && (x.DanhMucChaId == null || ids.Contains(x.DanhMucChaId.Value))).ToList();
            if (next.Count == 0) break;
            result.AddRange(next); foreach (var x in next) ids.Add(x.Id);
        }
        return result;
    }
    private static HashSet<Guid> Nhanh(List<DanhMuc> categories, Guid id)
    {
        var ids = new HashSet<Guid> { id };
        while (true)
        {
            var next = categories.Where(x => x.DanhMucChaId != null && ids.Contains(x.DanhMucChaId.Value) && !ids.Contains(x.Id)).Select(x => x.Id).ToList();
            if (next.Count == 0) return ids;
            foreach (var key in next) ids.Add(key);
        }
    }
    public async Task<List<DanhMucDto>> DanhMucAsync(CancellationToken ct)
    {
        var categories = await DanhMucHienAsync(ct);
        var counts = await MuaBan(CongKhai).GroupBy(x => x.SanPham.DanhMucId).Select(x => new { Id = x.Key, So = x.Count() }).ToListAsync(ct);
        return categories.Select(c => new DanhMucDto(c.Id, c.Ten, c.Ma, c.DanhMucChaId, counts.Where(x => Nhanh(categories, c.Id).Contains(x.Id)).Sum(x => x.So), c.BieuTuong)).ToList();
    }
    private async Task<List<TinDangDto>> DtoAsync(IQueryable<TinDang> query, bool own, CancellationToken ct)
    {
        var rows = await query.Include(t => t.SanPham).ThenInclude(s => s.DanhMuc).ToListAsync(ct);
        if (rows.Count == 0) return [];
        var products = rows.Select(t => t.SanPhamId).ToList();
        var users = rows.Select(t => t.NguoiDangId).Distinct().ToList();
        var ids = rows.Select(t => t.Id).ToList();
        var needs = await db.NhuCauTraoDoi.AsNoTracking().Where(x => ids.Contains(x.TinDangId)).ToDictionaryAsync(x => x.TinDangId, ct);
        var photos = await db.AnhSanPham.AsNoTracking().Where(x => products.Contains(x.SanPhamId)).OrderBy(x => x.ThuTu).ToListAsync(ct);
        var profiles = await db.HoSoCaNhan.AsNoTracking().Include(x => x.NguoiDung).Where(x => users.Contains(x.NguoiDungId)).ToDictionaryAsync(x => x.NguoiDungId, ct);
        var chats = own ? await db.CuocTroChuyen.Where(x => x.TinDangId != null && ids.Contains(x.TinDangId.Value))
            .GroupBy(x => x.TinDangId!.Value).Select(x => new { Id = x.Key, So = x.Count() }).ToDictionaryAsync(x => x.Id, x => x.So, ct) : [];
        var referenced = own ? await db.ChiTietDonHang.Where(x => products.Contains(x.SanPhamId)).Select(x => x.SanPhamId).ToListAsync(ct) : [];
        if (own)
        {
            var exchanges = await db.DeNghiTraoDoi.Where(x => products.Contains(x.SanPhamGuiId) || products.Contains(x.SanPhamNhanId)).Select(x => new { x.SanPhamGuiId, x.SanPhamNhanId }).ToListAsync(ct);
            referenced.AddRange(exchanges.SelectMany(x => new[] { x.SanPhamGuiId, x.SanPhamNhanId }));
            referenced.AddRange(await db.ThanhVienVong.Where(x => products.Contains(x.SanPhamId)).Select(x => x.SanPhamId).ToListAsync(ct));
            referenced.AddRange(await db.GiuChoSanPham.Where(x => products.Contains(x.SanPhamId) && !x.DaGiaiPhong).Select(x => x.SanPhamId).ToListAsync(ct));
        }
        return rows.Select(t => {
            var p = profiles.GetValueOrDefault(t.NguoiDangId);
            return new TinDangDto(t.Id, t.SanPhamId, t.SanPham.Ten, t.SanPham.DanhMucId, t.SanPham.DanhMuc.Ten,
                t.GiaBan, t.SanPham.GiaTriThamKhao, t.SanPham.TinhTrang.ToString(), t.TrangThai.ToString(), t.LoaiTin.ToString(),
                t.TinhThanh, t.DiaDiem, t.SanPham.MoTa, t.NguoiDangId, p?.HoTen ?? "Thành viên ReMarket", p?.DaXacMinhDienThoai ?? false,
                p?.NguoiDung.NgayTao ?? t.NgayTao, t.NgayDang, Convert.ToBase64String(t.PhienBan),
                photos.Where(x => x.SanPhamId == t.SanPhamId).Select(x => new AnhTinDto(x.Id, $"api/tindang/{t.Id}/anh/{x.Id}", x.ThuTu, x.LaAnhBia)).ToList(),
                t.LuotXem, chats.GetValueOrDefault(t.Id), own ? t.LyDoTuChoi : null,
                own && t.TrangThai is not (TrangThaiTinDang.DaBan or TrangThaiTinDang.DaTraoDoi) && !referenced.Contains(t.SanPhamId),
                needs.TryGetValue(t.Id,out var need) ? new NhuCauTraoDoiDto(need.MoTaMongMuon,need.ChoPhepMotMot,need.ChoPhepSmartSwap,need.ChoPhepBuTien,need.SoTienBuToiDa,need.GhiChu,
                    new(need.DanhMucMongMuonId,need.TuKhoaMongMuon,need.TinhTrangToiThieu?.ToString(),need.GiaTriMongMuonTu,need.GiaTriMongMuonDen)) : null);
        }).ToList();
    }
    public async Task<TrangTinDto> TimAsync(BoLocTin filter, CancellationToken ct, Guid? owner=null)
    {
        if (filter.GiaTu > filter.GiaDen) throw new LoiTaiKhoan(400, "Giá từ không được lớn hơn giá đến.");
        if (filter.LoaiTin is not ("MuaBan" or "TraoDoi")) throw new LoiTaiKhoan(400, "Loại tin không hợp lệ.");
        var query = filter.LoaiTin == "MuaBan" ? MuaBan(CongKhai) : CongKhai.Where(t => t.LoaiTin == LoaiTinDang.TraoDoi || t.LoaiTin == LoaiTinDang.CaHai);
        if(owner!=null)query=query.Where(t=>t.NguoiDangId==owner);
        if(filter.LoaiTin=="TraoDoi") {
            query=query.Where(t=>!db.GiuChoSanPham.Any(h=>h.SanPhamId==t.SanPhamId&&!h.DaGiaiPhong));
            if(filter.ChoPhepBuTien is bool bu)query=query.Where(t=>db.NhuCauTraoDoi.Any(n=>n.TinDangId==t.Id&&n.ChoPhepBuTien==bu));
            if(filter.ChoPhepMotMot is bool mot)query=query.Where(t=>db.NhuCauTraoDoi.Any(n=>n.TinDangId==t.Id&&n.ChoPhepMotMot==mot));
            if(filter.ChoPhepSmartSwap is bool smart)query=query.Where(t=>db.NhuCauTraoDoi.Any(n=>n.TinDangId==t.Id&&n.ChoPhepSmartSwap==smart));
        }
        var categories = await DanhMucHienAsync(ct);
        if (filter.DanhMucId is Guid id && !categories.Any(x => x.Id == id)) throw new LoiTaiKhoan(400, "Danh mục không khả dụng.");
        var allowed = filter.DanhMucId is Guid category ? Nhanh(categories, category) : categories.Select(x => x.Id).ToHashSet();
        query = query.Where(t => allowed.Contains(t.SanPham.DanhMucId));
        if (!string.IsNullOrWhiteSpace(filter.TuKhoa))
        {
            var text = filter.TuKhoa.Trim();
            query = query.Where(t => EF.Functions.Collate(t.SanPham.Ten, "Latin1_General_100_CI_AI").Contains(text) || EF.Functions.Collate(t.SanPham.MoTa, "Latin1_General_100_CI_AI").Contains(text) || (filter.LoaiTin=="TraoDoi"&&db.NhuCauTraoDoi.Any(n=>n.TinDangId==t.Id&&EF.Functions.Collate(n.MoTaMongMuon,"Latin1_General_100_CI_AI").Contains(text))));
        }
        if (filter.GiaTu != null) query = query.Where(t => (filter.LoaiTin=="TraoDoi"?t.SanPham.GiaTriThamKhao:t.GiaBan ?? t.SanPham.GiaTriThamKhao) >= filter.GiaTu);
        if (filter.GiaDen != null) query = query.Where(t => (filter.LoaiTin=="TraoDoi"?t.SanPham.GiaTriThamKhao:t.GiaBan ?? t.SanPham.GiaTriThamKhao) <= filter.GiaDen);
        if (!string.IsNullOrWhiteSpace(filter.TinhThanh)) { var city = filter.TinhThanh.Trim(); query = query.Where(t => EF.Functions.Collate(t.TinhThanh, "Latin1_General_100_CI_AI").Contains(city)); }
        if (!string.IsNullOrEmpty(filter.TinhTrang))
        {
            if (!Enum.TryParse<TinhTrangSanPham>(filter.TinhTrang, out var condition) || !Enum.IsDefined(condition)) throw new LoiTaiKhoan(400, "Tình trạng không hợp lệ.");
            query = query.Where(t => t.SanPham.TinhTrang == condition);
        }
        query = filter.SapXep switch {
            "moinhat" => query.OrderByDescending(t => t.NgayDang).ThenByDescending(t => t.Id),
            "giatang" => query.OrderBy(t => filter.LoaiTin=="TraoDoi"?t.SanPham.GiaTriThamKhao:t.GiaBan ?? t.SanPham.GiaTriThamKhao).ThenBy(t => t.Id),
            "giagiam" => query.OrderByDescending(t => filter.LoaiTin=="TraoDoi"?t.SanPham.GiaTriThamKhao:t.GiaBan ?? t.SanPham.GiaTriThamKhao).ThenBy(t => t.Id),
            _ => throw new LoiTaiKhoan(400, "Sắp xếp không hợp lệ.")
        };
        var count = await query.CountAsync(ct);
        return new(await DtoAsync(query.Skip((filter.Trang - 1) * filter.KichThuocTrang).Take(filter.KichThuocTrang), false, ct), count,
            filter.Trang, filter.KichThuocTrang, (int)Math.Ceiling(count / (double)filter.KichThuocTrang));
    }
    public async Task<TrangChuSanPhamDto> TrangChuAsync(CancellationToken ct)
    {
        var categories = await DanhMucAsync(ct);
        var allowed = categories.Select(x => x.Id).ToList();
        var query = MuaBan(CongKhai).Where(x => allowed.Contains(x.SanPham.DanhMucId));
        return new(categories, await DtoAsync(query.OrderByDescending(x => x.LuotXem).ThenByDescending(x => x.NgayDang).ThenBy(x => x.Id).Take(6), false, ct),
            await DtoAsync(query.OrderByDescending(x => x.NgayDang).ThenByDescending(x => x.Id).Take(6), false, ct));
    }
    public async Task<TinDangDto> ChiTietAsync(Guid id, Guid? viewer, bool manage, CancellationToken ct)
    {
        var query = manage ? db.TinDang.AsNoTracking().Where(t => t.Id == id && t.NguoiDangId == viewer && t.NgayXoa == null) : CongKhai.Where(t => t.Id == id);
        if (!manage)
        {
            var allowed = (await DanhMucHienAsync(ct)).Select(x => x.Id).ToList();
            query = query.Where(t => allowed.Contains(t.SanPham.DanhMucId));
        }
        var result = (await DtoAsync(query, manage, ct)).SingleOrDefault() ?? throw new LoiTaiKhoan(404, "Tin không còn hiển thị hoặc bạn không có quyền xem.");
        // Public views never change the aggregate rowversion (which protects form writes).
        return result;
    }
    public async Task<TinCuaToiDto> CuaToiAsync(Guid owner, string? status, int page, int size, CancellationToken ct, string loaiTin="MuaBan")
    {
        if(loaiTin is not ("MuaBan" or "TraoDoi"))throw new LoiTaiKhoan(400,"Loại tin không hợp lệ.");
        var query = db.TinDang.AsNoTracking().Where(t => t.NguoiDangId == owner && t.NgayXoa == null && (loaiTin=="MuaBan"?t.LoaiTin==LoaiTinDang.MuaBan:(t.LoaiTin==LoaiTinDang.TraoDoi||t.LoaiTin==LoaiTinDang.CaHai)));
        var stats = await query.GroupBy(t => t.TrangThai).Select(g => new ThongKeTinDto(g.Key.ToString(), g.Count())).ToListAsync(ct);
        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<TrangThaiTinDang>(status, out var state) || !Enum.IsDefined(state)) throw new LoiTaiKhoan(400, "Trạng thái không hợp lệ.");
            query = query.Where(t => t.TrangThai == state);
        }
        var count = await query.CountAsync(ct);
        var list = await DtoAsync(query.OrderByDescending(t => t.NgayCapNhat ?? t.NgayTao).ThenByDescending(t => t.Id).Skip((page - 1) * size).Take(size), true, ct);
        return new(new(list, count, page, size, (int)Math.Ceiling(count / (double)size)), stats);
    }
    private async Task KiemTraFormAsync(LuuTinDangRequest form, CancellationToken ct)
    {
        if (form.Ten.Trim().Length < 3 || form.MoTa.Trim().Length < 10 || form.TinhThanh.Trim().Length < 2 || form.DiaDiem.Trim().Length < (form.TraoDoi==null?5:2))
            throw new LoiTaiKhoan(400, "Vui lòng nhập đầy đủ tên, mô tả và địa điểm.");
        if (!(await DanhMucHienAsync(ct)).Any(x => x.Id == form.DanhMucId)) throw new LoiTaiKhoan(400, "Vui lòng chọn danh mục đang hiển thị.");
        if (!Enum.TryParse<TinhTrangSanPham>(form.TinhTrang, out var condition) || !Enum.IsDefined(condition)) throw new LoiTaiKhoan(400, "Tình trạng không hợp lệ.");
        if(form.TraoDoi is {} n && (n.MoTaMongMuon.Trim().Length<10||(!n.ChoPhepMotMot&&!n.ChoPhepSmartSwap)||(!n.ChoPhepBuTien&&n.SoTienBuToiDa!=0)))
            throw new LoiTaiKhoan(400,"Nhập món muốn nhận, chọn hình thức trao đổi và mức bù hợp lệ.");
        if (form.TraoDoi?.TieuChiSmartSwap is {} criteria)
        {
            if (criteria.GiaTriMongMuonTu > criteria.GiaTriMongMuonDen || criteria.GiaTriMongMuonTu is <0 or >1_000_000_000_000 || criteria.GiaTriMongMuonDen is <0 or >1_000_000_000_000)
                throw new LoiTaiKhoan(400, "Khoảng giá trị mong muốn không hợp lệ.");
            if (criteria.TinhTrangToiThieu != null && (!Enum.TryParse<TinhTrangSanPham>(criteria.TinhTrangToiThieu, out var minimum) || !Enum.IsDefined(minimum)))
                throw new LoiTaiKhoan(400, "Tình trạng tối thiểu không hợp lệ.");
            if (criteria.DanhMucMongMuonId is Guid wanted && !(await DanhMucHienAsync(ct)).Any(x => x.Id == wanted))
                throw new LoiTaiKhoan(400, "Danh mục muốn nhận không khả dụng.");
        }
    }
    private static void ApDungTieuChi(NhuCauTraoDoi need, TieuChiSmartSwapRequest? criteria)
    {
        // Omission by existing Android clients preserves criteria; an explicit empty object clears them.
        if (criteria == null) return;
        need.DanhMucMongMuonId = criteria.DanhMucMongMuonId;
        need.TuKhoaMongMuon = criteria.TuKhoaMongMuon?.Trim() is { Length: >0 } words ? words : null;
        need.TinhTrangToiThieu = criteria.TinhTrangToiThieu == null ? null : Enum.Parse<TinhTrangSanPham>(criteria.TinhTrangToiThieu);
        need.GiaTriMongMuonTu = criteria.GiaTriMongMuonTu;
        need.GiaTriMongMuonDen = criteria.GiaTriMongMuonDen;
    }
    public async Task<TinDangDto> TaoAsync(Guid owner, LuuTinDangRequest form, CancellationToken ct)
    {
        await KiemTraFormAsync(form, ct);
        var product = new ReMarket.Domain.Models.SanPham { ChuSoHuuId = owner, DanhMucId = form.DanhMucId,
            Ten = form.Ten.Trim(), MoTa = form.MoTa.Trim(), GiaTriThamKhao = form.GiaBan, TinhTrang = Enum.Parse<TinhTrangSanPham>(form.TinhTrang) };
        var listing = new TinDang { SanPhamId = product.Id, NguoiDangId = owner, LoaiTin = form.TraoDoi==null?LoaiTinDang.MuaBan:LoaiTinDang.TraoDoi, GiaBan = form.TraoDoi==null?form.GiaBan:null,
            TrangThai = TrangThaiTinDang.BanNhap, TinhThanh = form.TinhThanh.Trim(), DiaDiem = form.DiaDiem.Trim(), NgayCapNhat = DateTimeOffset.UtcNow };
        db.Add(product); db.Add(listing);
        if(form.TraoDoi is {} n){var need=new NhuCauTraoDoi {TinDangId=listing.Id,MoTaMongMuon=n.MoTaMongMuon.Trim(),ChoPhepMotMot=n.ChoPhepMotMot,ChoPhepSmartSwap=n.ChoPhepSmartSwap,ChoPhepBuTien=n.ChoPhepBuTien,SoTienBuToiDa=n.SoTienBuToiDa,GhiChu=n.GhiChu?.Trim()};ApDungTieuChi(need,n.TieuChiSmartSwap);db.Add(need);}
        await db.SaveChangesAsync(ct);
        return await ChiTietAsync(listing.Id, owner, true, ct);
    }
    private async Task<TinDang> KhoaAsync(Guid id, Guid owner, string version, bool editing, CancellationToken ct)
    {
        var (listing, _) = await stock.TinAsync(id, ct);
        if (listing is null || listing.NguoiDangId != owner || listing.NgayXoa != null) throw new LoiTaiKhoan(404, "Không tìm thấy tin của bạn.");
        byte[] expected;
        try { expected = Convert.FromBase64String(version); } catch (FormatException) { throw new LoiTaiKhoan(400, "Phiên bản tin không hợp lệ."); }
        if (!listing.PhienBan.SequenceEqual(expected)) throw new LoiTaiKhoan(409, "Tin vừa thay đổi. Hãy tải lại trước khi thao tác.");
        if (await db.GiuChoSanPham.AnyAsync(x => x.SanPhamId == listing.SanPhamId && !x.DaGiaiPhong, ct))
            throw new LoiTaiKhoan(409, "Sản phẩm đang được giữ cho giao dịch. Hãy giải quyết giao dịch trước.");
        if (editing)
        {
            if (listing.TrangThai is TrangThaiTinDang.DaBan or TrangThaiTinDang.DaTraoDoi) throw new LoiTaiKhoan(409, "Không sửa nội dung tin đã bán hoặc đã trao đổi.");
            if (await db.ChiTietDonHang.AnyAsync(x => x.SanPhamId == listing.SanPhamId, ct) ||
                await db.DeNghiTraoDoi.AnyAsync(x => x.SanPhamGuiId == listing.SanPhamId || x.SanPhamNhanId == listing.SanPhamId, ct) ||
                await db.ThanhVienVong.AnyAsync(x => x.SanPhamId == listing.SanPhamId, ct))
                throw new LoiTaiKhoan(409, "Nội dung đã được giao dịch tham chiếu, không thể ghi đè lịch sử.");
        }
        listing.NgayCapNhat = DateTimeOffset.UtcNow;
        db.Entry(listing).Property(x => x.NgayCapNhat).IsModified = true;
        return listing;
    }
    private static void SuaCanDuyet(TinDang listing)
    {
        if (listing.TrangThai == TrangThaiTinDang.DangHienThi) listing.TrangThai = TrangThaiTinDang.ChoDuyet;
        listing.NguoiDuyetId = null; listing.LyDoTuChoi = null;
    }
    public async Task<TinDangDto> SuaAsync(Guid id, Guid owner, LuuTinDangRequest form, CancellationToken ct)
    {
        await KiemTraFormAsync(form, ct);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var listing = await KhoaAsync(id, owner, form.PhienBan ?? "", true, ct);
        if((listing.LoaiTin==LoaiTinDang.MuaBan)!=(form.TraoDoi==null))throw new LoiTaiKhoan(409,"Không đổi loại tin đã tạo. Hãy sử dụng form tương ứng.");
        var product = await db.SanPham.SingleAsync(x => x.Id == listing.SanPhamId, ct);
        product.Ten = form.Ten.Trim(); product.MoTa = form.MoTa.Trim(); product.DanhMucId = form.DanhMucId;
        product.GiaTriThamKhao = form.GiaBan; product.TinhTrang = Enum.Parse<TinhTrangSanPham>(form.TinhTrang);
        listing.GiaBan = listing.LoaiTin==LoaiTinDang.TraoDoi?null:form.GiaBan; listing.DiaDiem = form.DiaDiem.Trim(); listing.TinhThanh = form.TinhThanh.Trim();
        if(form.TraoDoi is {} n){var need=await db.NhuCauTraoDoi.SingleAsync(x=>x.TinDangId==id,ct);need.MoTaMongMuon=n.MoTaMongMuon.Trim();need.ChoPhepMotMot=n.ChoPhepMotMot;need.ChoPhepSmartSwap=n.ChoPhepSmartSwap;need.ChoPhepBuTien=n.ChoPhepBuTien;need.SoTienBuToiDa=n.SoTienBuToiDa;need.GhiChu=n.GhiChu?.Trim();ApDungTieuChi(need,n.TieuChiSmartSwap);}
        SuaCanDuyet(listing);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await ChiTietAsync(id, owner, true, ct);
    }
    public async Task<TinDangDto> ChuyenAsync(Guid id, Guid owner, string version, string action, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var listing = await KhoaAsync(id, owner, version, false, ct);
        switch (action)
        {
            case "gui-duyet":
                if (listing.TrangThai is not (TrangThaiTinDang.BanNhap or TrangThaiTinDang.TuChoi or TrangThaiTinDang.DaAn))
                    throw new LoiTaiKhoan(409, "Chỉ gửi duyệt bản nháp, tin bị từ chối hoặc đã ẩn.");
                if (!await db.AnhSanPham.AnyAsync(x => x.SanPhamId == listing.SanPhamId && x.LaAnhBia, ct)) throw new LoiTaiKhoan(400, "Cần ít nhất một ảnh và chọn ảnh bìa trước khi gửi duyệt.");
                var product = await db.SanPham.SingleAsync(x => x.Id == listing.SanPhamId, ct);
                var need=await db.NhuCauTraoDoi.SingleOrDefaultAsync(x=>x.TinDangId==id,ct);
                await KiemTraFormAsync(new(product.Ten, product.DanhMucId, listing.GiaBan ?? product.GiaTriThamKhao, product.TinhTrang.ToString(), product.MoTa, listing.TinhThanh, listing.DiaDiem, version,need==null?null:new(need.MoTaMongMuon,need.ChoPhepMotMot,need.ChoPhepSmartSwap,need.ChoPhepBuTien,need.SoTienBuToiDa,need.GhiChu,
                    new(need.DanhMucMongMuonId,need.TuKhoaMongMuon,need.TinhTrangToiThieu?.ToString(),need.GiaTriMongMuonTu,need.GiaTriMongMuonDen))), ct);
                if(listing.LoaiTin!=LoaiTinDang.MuaBan&&need==null)throw new LoiTaiKhoan(400,"Thiếu nhu cầu trao đổi.");
                listing.TrangThai = TrangThaiTinDang.ChoDuyet; listing.LyDoTuChoi = null; listing.NguoiDuyetId = null;
                break;
            case "an":
                if (listing.TrangThai is not (TrangThaiTinDang.DangHienThi or TrangThaiTinDang.ChoDuyet)) throw new LoiTaiKhoan(409, "Chỉ ẩn tin đang bán hoặc chờ duyệt.");
                listing.TrangThai = TrangThaiTinDang.DaAn; break;
            case "da-ban":
                if(listing.LoaiTin==LoaiTinDang.TraoDoi)throw new LoiTaiKhoan(409,"Tin trao đổi hoàn tất qua xác nhận hai bên.");
                if (listing.TrangThai is not (TrangThaiTinDang.DangHienThi or TrangThaiTinDang.DaAn)) throw new LoiTaiKhoan(409, "Chỉ xác nhận đã bán tin đang bán hoặc đã ẩn.");
                listing.TrangThai = TrangThaiTinDang.DaBan; break;
            case "xoa": listing.NgayXoa = DateTimeOffset.UtcNow; listing.TrangThai = listing.TrangThai is TrangThaiTinDang.DaBan or TrangThaiTinDang.DaTraoDoi ? listing.TrangThai : TrangThaiTinDang.DaAn; break;
            default: throw new LoiTaiKhoan(400, "Thao tác không hợp lệ.");
        }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        // Deleted records remain for FK/history; callers of DELETE do not receive this DTO.
        if (listing.NgayXoa != null) return null!;
        return await ChiTietAsync(id, owner, true, ct);
    }
    public async Task<TinDangDto> ThemAnhAsync(Guid id, Guid owner, string version, Stream stream, CancellationToken ct)
    {
        await ChiTietAsync(id, owner, true, ct);
        var file = await images.LuuAsync(stream, ct);
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            var listing = await KhoaAsync(id, owner, version, true, ct);
            var photos = await db.AnhSanPham.Where(x => x.SanPhamId == listing.SanPhamId).ToListAsync(ct);
            if (photos.Count >= (listing.LoaiTin==LoaiTinDang.MuaBan?8:6)) throw new LoiTaiKhoan(400, "Tin mua bán tối đa 8 ảnh; tin trao đổi tối đa 6 ảnh.");
            db.Add(new AnhSanPham { SanPhamId = listing.SanPhamId, DuongDan = file, ThuTu = photos.Count == 0 ? 0 : photos.Max(x => x.ThuTu) + 1, LaAnhBia = photos.Count == 0 });
            SuaCanDuyet(listing);
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        }
        catch { images.Xoa(file); throw; }
        return await ChiTietAsync(id, owner, true, ct);
    }
    public async Task<TinDangDto> SuaAnhAsync(Guid id, Guid photoId, Guid owner, string version, bool delete, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var listing = await KhoaAsync(id, owner, version, true, ct);
        var photos = await db.AnhSanPham.Where(x => x.SanPhamId == listing.SanPhamId).OrderBy(x => x.ThuTu).ToListAsync(ct);
        var photo = photos.SingleOrDefault(x => x.Id == photoId) ?? throw new LoiTaiKhoan(404, "Ảnh không thuộc tin này.");
        if (!delete && photo.LaAnhBia) return await ChiTietAsync(id, owner, true, ct);
        if (!delete)
        {
            foreach (var previous in photos.Where(x => x.LaAnhBia)) previous.LaAnhBia = false;
            // SQL's filtered unique index is immediate, so clear the old cover before setting the new one.
            await db.SaveChangesAsync(ct);
            photo.LaAnhBia = true;
        }
        else
        {
            db.Remove(photo); await db.SaveChangesAsync(ct);
            if (photo.LaAnhBia && photos.Count > 1) photos.First(x => x.Id != photoId).LaAnhBia = true;
        }
        SuaCanDuyet(listing);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        if (delete) images.Xoa(photo.DuongDan);
        return await ChiTietAsync(id, owner, true, ct);
    }
    public async Task<string> AnhAsync(Guid id, Guid photoId, Guid? viewer, bool admin, CancellationToken ct)
    {
        var listing = await db.TinDang.AsNoTracking().Where(t => t.Id == id && t.NgayXoa == null).SingleOrDefaultAsync(ct);
        if (listing is null) throw new LoiTaiKhoan(404, "Không tìm thấy ảnh.");
        if (listing.NguoiDangId != viewer && !admin) await ChiTietAsync(id, viewer, false, ct);
        var photo = await db.AnhSanPham.AsNoTracking().SingleOrDefaultAsync(x => x.Id == photoId && x.SanPhamId == listing.SanPhamId, ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy ảnh.");
        var path = images.DuongDan(photo.DuongDan);
        if (!File.Exists(path)) throw new LoiTaiKhoan(404, "Ảnh không còn tồn tại.");
        return path;
    }
    public async Task<TrangTinDto> ChoDuyetAsync(int page, int size, CancellationToken ct)
    {
        var query = db.TinDang.AsNoTracking().Where(t => t.NgayXoa == null && t.TrangThai == TrangThaiTinDang.ChoDuyet);
        var count = await query.CountAsync(ct);
        return new(await DtoAsync(query.OrderBy(t => t.NgayCapNhat).ThenBy(t => t.Id).Skip((page - 1) * size).Take(size), false, ct), count, page, size, (int)Math.Ceiling(count / (double)size));
    }
    // These projections are called only after ADMIN authorization in QuanTriService.
    public async Task<TinDangDto> QuanTriDocAsync(Guid id,CancellationToken ct)=>
        (await DtoAsync(db.TinDang.AsNoTracking().Where(x=>x.Id==id&&x.NgayXoa==null),true,ct)).SingleOrDefault()??throw new LoiTaiKhoan(404,"Không tìm thấy tin.");
    public async Task<TrangTinDto> QuanTriTimAsync(string? q,string? status,int page,CancellationToken ct)
    {
        var query=db.TinDang.AsNoTracking().Where(x=>x.NgayXoa==null);
        if(!string.IsNullOrWhiteSpace(q)){var text=q.Trim();query=query.Where(x=>EF.Functions.Collate(x.SanPham.Ten,"Latin1_General_100_CI_AI").Contains(text)||x.Id.ToString().Contains(text));}
        if(!string.IsNullOrEmpty(status)){if(!Enum.TryParse<TrangThaiTinDang>(status,out var value)||!Enum.IsDefined(value))throw new LoiTaiKhoan(400,"Trạng thái tin không hợp lệ.");query=query.Where(x=>x.TrangThai==value);}
        var count=await query.CountAsync(ct);return new(await DtoAsync(query.OrderBy(x=>x.NgayCapNhat).ThenBy(x=>x.Id).Skip((page-1)*12).Take(12),false,ct),count,page,12,(int)Math.Ceiling(count/12d));
    }
    public async Task<TinDangDto> DuyetAsync(Guid id, Guid admin, DuyetTinRequest form, CancellationToken ct)
    {
        var owner = await db.TinDang.Where(t => t.Id == id && t.NgayXoa == null).Select(t => (Guid?)t.NguoiDangId).SingleOrDefaultAsync(ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy tin.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var listing = await KhoaAsync(id, owner, form.PhienBan, false, ct);
        if (owner == admin) throw new LoiTaiKhoan(403, "Không tự duyệt tin của mình.");
        if (listing.TrangThai != TrangThaiTinDang.ChoDuyet) throw new LoiTaiKhoan(409, "Tin không còn chờ duyệt.");
        if (!form.ChapNhan && string.IsNullOrWhiteSpace(form.LyDo)) throw new LoiTaiKhoan(400, "Cần lý do từ chối tin.");
        if (form.ChapNhan)
        {
            var category = await db.SanPham.Where(x => x.Id == listing.SanPhamId).Select(x => x.DanhMucId).SingleAsync(ct);
            if (!(await DanhMucHienAsync(ct)).Any(x => x.Id == category) || !await db.AnhSanPham.AnyAsync(x => x.SanPhamId == listing.SanPhamId && x.LaAnhBia, ct))
                throw new LoiTaiKhoan(409, "Cần ảnh bìa và danh mục khả dụng để duyệt.");
        }
        var before = listing.TrangThai;
        listing.TrangThai = form.ChapNhan ? TrangThaiTinDang.DangHienThi : TrangThaiTinDang.TuChoi;
        listing.NguoiDuyetId = admin; listing.LyDoTuChoi = form.ChapNhan ? null : form.LyDo!.Trim();
        if (form.ChapNhan) listing.NgayDang = DateTimeOffset.UtcNow;
        db.Add(new NhatKyKiemDuyet { TinDangId = id, QuanTriVienId = admin, TrangThaiTruoc = before, TrangThaiSau = listing.TrangThai, LyDo = form.ChapNhan ? "Duyệt tin mua bán" : form.LyDo!.Trim() });
        db.Add(new ThongBao{NguoiDungId=owner,Loai=LoaiThongBao.TinDang,TieuDe=form.ChapNhan?"Tin đã được duyệt":"Tin cần chỉnh sửa",NoiDung=form.ChapNhan?"Tin của bạn đã được kiểm duyệt và hiển thị.":form.LyDo!.Trim()});
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return await ChiTietAsync(id, owner, true, ct);
    }
}
