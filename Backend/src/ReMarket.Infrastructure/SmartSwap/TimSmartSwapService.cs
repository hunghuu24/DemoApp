using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed class TimSmartSwapService(ReMarketDbContext db, IOptions<GioiHanTimSmartSwap> options, TimeProvider clock)
{
    public async Task<KetQuaTimSmartSwapDto> TimAsync(TimSmartSwapRequest filter, CancellationToken ct)
    {
        var snapshot = clock.GetUtcNow(); var limits = options.Value;
        var (candidates,categories)=await TaiAsync(filter,null,null,limits.SoTinToiDa+1,ct);
        if (candidates.Count > limits.SoTinToiDa) throw new LoiTaiKhoan(422,new VuotGioiHanSmartSwapException().Message);
        if (filter.TinDangId is Guid anchor && candidates.All(x => x.TinDangId != anchor))
            throw new LoiTaiKhoan(404,"Tin được chọn không còn khả dụng cho Smart Swap trong bộ lọc này.");
        if(filter.TinDangId==null && (filter.BuToiDa!=null || filter.TinhTrangToiThieu!=null || !string.IsNullOrWhiteSpace(filter.TuKhoaMuonNhan)))
            throw new LoiTaiKhoan(400,"Chọn sản phẩm của bạn trước khi lọc món muốn nhận và tiền bù.");
        try {
            var result=BoTimVongSmartSwap.Tim(candidates,categories,filter,limits,snapshot,ct);
            var users=result.Items.SelectMany(x=>x.ThanhVien).Select(x=>x.NguoiDungId).Distinct().ToArray();
            var profiles=await db.HoSoCaNhan.AsNoTracking().Where(x=>users.Contains(x.NguoiDungId))
                .Select(x=>new HoSoThanhVienSmartSwapDto(x.NguoiDungId,x.HoTen)).ToListAsync(ct);
            return result with {HoSo=profiles};
        }
        catch (VuotGioiHanSmartSwapException ex) { throw new LoiTaiKhoan(422,ex.Message); }
    }
    internal async Task<VongSmartSwapDeXuatDto> KiemTraVongAsync(Guid[] ids,string key,Guid? ownTransaction,CancellationToken ct)
    {
        var(rows,categories)=await TaiAsync(new(),ids,ownTransaction,3,ct);
        if(rows.Count!=3)throw new LoiTaiKhoan(409,"Một trong ba tin không còn khả dụng cho Smart Swap.");
        var found=BoTimVongSmartSwap.Tim(rows,categories,new(){KichThuocTrang=50},
            new(){SoTinToiDa=3,SoCanhToiDa=6,SoLuotDuyetToiDa=18},clock.GetUtcNow(),ct);
        return found.Items.SingleOrDefault(x=>x.MaDeXuat==key)??throw new LoiTaiKhoan(409,"Chiều trao đổi hoặc nhu cầu/giá/tiền bù không còn phù hợp.");
    }
    internal async Task<List<VongSmartSwapDeXuatDto>> GoiYBaTinAsync(Guid[] ids,CancellationToken ct)
    {
        var(rows,categories)=await TaiAsync(new(),ids,null,3,ct);
        return BoTimVongSmartSwap.Tim(rows,categories,new(){KichThuocTrang=50},new(){SoTinToiDa=3,SoCanhToiDa=6,SoLuotDuyetToiDa=18},clock.GetUtcNow(),ct).Items;
    }
    private async Task<(List<UngVienSmartSwap> Rows,List<DanhMucSmartSwap> Categories)> TaiAsync(TimSmartSwapRequest filter,Guid[]? ids,Guid? ownTransaction,int take,CancellationToken ct)
    {
        var allCategories = await db.DanhMuc.AsNoTracking().Where(x => x.DangHienThi)
            .Select(x => new DanhMucSmartSwap(x.Id,x.DanhMucChaId,x.Ten)).ToListAsync(ct);
        var allowed = new HashSet<Guid>();
        while (true)
        {
            var next = allCategories.Where(x => !allowed.Contains(x.Id) && (x.ChaId == null || allowed.Contains(x.ChaId.Value))).ToArray();
            if (next.Length == 0) break;
            foreach (var c in next) allowed.Add(c.Id);
        }
        var query = from need in db.NhuCauTraoDoi.AsNoTracking()
                    let t = need.TinDang
                    where need.ChoPhepSmartSwap && t.NgayXoa == null &&
                        (t.TrangThai == TrangThaiTinDang.DangHienThi || (ownTransaction!=null && t.TrangThai==TrangThaiTinDang.DangGiuCho &&
                            db.GiuChoSanPham.Any(h=>h.SanPhamId==t.SanPhamId&&!h.DaGiaiPhong&&h.GiaoDichId==ownTransaction))) &&
                        (t.LoaiTin == LoaiTinDang.TraoDoi || t.LoaiTin == LoaiTinDang.CaHai) &&
                        t.SanPham.ChuSoHuuId == t.NguoiDangId && t.SanPham.ChuSoHuu.TrangThai == TrangThaiNguoiDung.HoatDong &&
                        allowed.Contains(t.SanPham.DanhMucId) && (need.DanhMucMongMuonId == null || allowed.Contains(need.DanhMucMongMuonId.Value)) &&
                        t.SanPham.GiaTriThamKhao > 0 && t.SanPham.GiaTriThamKhao <= 1_000_000_000_000 &&
                        !db.GiuChoSanPham.Any(h => h.SanPhamId == t.SanPhamId && !h.DaGiaiPhong && h.GiaoDichId!=ownTransaction)
                    select need;
        if(ids!=null)query=query.Where(x=>ids.Contains(x.TinDangId));
        if(filter.ChiThanhVienDaXacMinh)query=query.Where(x=>db.HoSoCaNhan.Any(h=>h.NguoiDungId==x.TinDang.NguoiDangId&&h.DaXacMinhEmail&&h.DaXacMinhDienThoai));
        if (!string.IsNullOrWhiteSpace(filter.TinhThanh))
        {
            var city = filter.TinhThanh.Trim();
            query = query.Where(x => EF.Functions.Collate(x.TinDang.TinhThanh,"Latin1_General_100_CI_AI").Contains(city));
        }
        // Limit + 1 detects overflow. Never take a silent top-N subset before looking for cycles.
        var candidates = await query.OrderBy(x => x.TinDangId).Select(x => new UngVienSmartSwap(x.TinDangId,x.TinDang.SanPhamId,
            x.TinDang.NguoiDangId,x.TinDang.SanPham.Ten,x.TinDang.SanPham.DanhMucId,x.TinDang.SanPham.GiaTriThamKhao,
            x.TinDang.SanPham.TinhTrang,x.TinDang.TinhThanh,Convert.ToBase64String(x.TinDang.PhienBan),x.MoTaMongMuon,
            x.DanhMucMongMuonId,x.TuKhoaMongMuon,x.TinhTrangToiThieu,x.GiaTriMongMuonTu,x.GiaTriMongMuonDen,
            x.ChoPhepBuTien,x.SoTienBuToiDa)).Take(take).ToListAsync(ct);
        return (candidates,allCategories.Where(x=>allowed.Contains(x.Id)).ToList());
    }
}
