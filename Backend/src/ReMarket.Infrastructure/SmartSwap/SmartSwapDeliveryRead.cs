using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SmartSwap;
using ReMarket.Domain.Enums;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed partial class VongSmartSwapService
{
    private async Task<TienDoSmartSwapDto> TienDoAsync(Guid gd,Guid? version,TrangThaiVong state,CancellationToken ct)
    {
        var count=version==null?0:await db.XacNhanGiaoNhan.AsNoTracking().CountAsync(x=>x.GiaoDichId==gd&&x.PhienBanThoaThuanId==version&&x.Loai==LoaiXacNhan.DaNhan,ct);
        return new(count,3,$"{count}/3",state==TrangThaiVong.TamDung,state==TrangThaiVong.HoanTat);
    }
    public async Task<GiaoNhanSmartSwapDto> GiaoNhanAsync(Guid id,Guid actor,CancellationToken ct)
    {
        var v=await CuaToiAsync(id,actor,ct);var gd=await db.GiaoDich.AsNoTracking().SingleAsync(x=>x.Id==v.GiaoDichId,ct);
        var p=await db.PhienBanThoaThuan.AsNoTracking().Where(x=>x.GiaoDichId==v.GiaoDichId).OrderByDescending(x=>x.SoPhienBan).FirstOrDefaultAsync(ct);
        var members=await db.ThanhVienVong.AsNoTracking().Where(x=>x.VongSmartSwapId==id).ToListAsync(ct);var ids=members.Select(x=>x.Id).ToArray();
        var profiles=await HoSoAsync(members.Select(x=>x.NguoiDungId),ct);string Name(Guid user)=>profiles.FirstOrDefault(x=>x.NguoiDungId==user)?.HoTen??"Thành viên ReMarket";
        var contacts=await db.ThongTinGiaoNhanSmartSwap.AsNoTracking().Where(x=>ids.Contains(x.ThanhVienVongId)).ToDictionaryAsync(x=>x.ThanhVienVongId,ct);
        var contactRows=members.OrderBy(x=>x.ViTri).Select(m=>{contacts.TryGetValue(m.Id,out var row);var own=m.NguoiDungId==actor;return new LienHeSmartSwapDto(m.NguoiDungId,Name(m.NguoiDungId),
            row!=null&&row.PhienBanThoaThuanId==p?.Id,own&&row!=null?Convert.ToBase64String(row.PhienBan):null,
            own?row?.TenLienHe:null,own?row?.SoDienThoai:null,own?row?.DiaChiGui:null,own?row?.DiaChiNhan:null);}).ToList();
        var legs=await db.ChangTraoDoi.AsNoTracking().Where(x=>x.VongSmartSwapId==id).ToDictionaryAsync(x=>x.Id,ct);
        var ships=await db.VanChuyenMoPhong.AsNoTracking().Where(x=>x.ChangTraoDoi!.VongSmartSwapId==id).OrderBy(x=>x.ChangTraoDoi!.ThanhVienGiao.ViTri).ToListAsync(ct);
        var shipIds=ships.Select(x=>x.Id).ToArray();var histories=await db.LichSuVanChuyen.AsNoTracking().Where(x=>shipIds.Contains(x.VanChuyenMoPhongId)).OrderBy(x=>x.SoThuTu).ToListAsync(ct);
        var productIds=members.Select(m=>m.SanPhamId).ToArray();
        var products=await db.SanPham.AsNoTracking().Where(x=>productIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var reports=await db.KhieuNai.AsNoTracking().Where(x=>x.GiaoDichId==v.GiaoDichId&&x.VanChuyenMoPhongId!=null).OrderBy(x=>x.NgayTao).ThenBy(x=>x.Id)
            .Select(x=>new SuCoSmartSwapDto(x.Id,x.VanChuyenMoPhongId!.Value,x.NguoiGuiId,x.Ma,x.LoaiSuCo!,x.TieuDe,x.NoiDung,x.TrangThai.ToString(),x.NgayTao)).ToListAsync(ct);
        var active=v.TrangThai==TrangThaiVong.DangBanGiao&&gd.TrangThai==TrangThaiGiaoDich.DangThucHien&&reports.All(x=>x.TrangThai is "DaGiaiQuyet" or "TuChoi");
        var rows=ships.Select(s=>{var leg=legs[s.ChangTraoDoiId!.Value];var sender=members.Single(x=>x.Id==leg.ThanhVienGiaoId).NguoiDungId;var receiver=members.Single(x=>x.Id==leg.ThanhVienNhanId).NguoiDungId;var related=actor==sender||actor==receiver;
            return new VanChuyenSmartSwapDto(s.Id,leg.Id,sender,receiver,leg.SanPhamId,products[leg.SanPhamId].Ten,s.MaVanDon,s.MaVanDonTuNhap,s.TrangThai.ToString(),Convert.ToBase64String(s.PhienBan),s.NgayNhanDungHang,
                related?s.TenNguoiGui:null,related?s.DienThoaiGui:null,related?s.DiaChiGui:null,related?s.TenNguoiNhan:null,related?s.DienThoaiNhan:null,related?s.DiaChiNhan:null,
                histories.Where(h=>h.VanChuyenMoPhongId==s.Id).Select(h=>new LichSuGuiHangSmartSwapDto(h.SoThuTu,h.TrangThai.ToString(),h.MoTa,h.ThoiDiem)).ToList(),
                active&&actor==sender&&s.TrangThai==TrangThaiVanChuyen.ChoGui,active&&actor==sender&&s.TrangThai!=TrangThaiVanChuyen.DaGiao,
                active&&actor==receiver&&s.TrangThai==TrangThaiVanChuyen.DaGiao&&s.NgayNhanDungHang==null,
                actor==receiver&&s.NgayNhanDungHang==null&&(active||v.TrangThai==TrangThaiVong.TamDung&&gd.TrangThai==TrangThaiGiaoDich.TranhChap));}).ToList();
        var evidence=await db.BangChungGiaoNhan.AsNoTracking().Where(x=>x.XacNhanGiaoNhan.GiaoDichId==v.GiaoDichId).OrderBy(x=>x.NgayTao)
            .Select(x=>new BangChungSmartSwapDto(x.Id,x.NguoiTaiLenId,$"api/smart-swaps/{id}/evidence/{x.Id}")).ToListAsync(ct);
        return new(id,p?.Id,v.TrangThai.ToString(),gd.TrangThai.ToString(),contactRows.Count(x=>x.DaXacNhan),await TienDoAsync(v.GiaoDichId,p?.Id,v.TrangThai,ct),contactRows,rows,reports,
            v.NguoiKhoiTaoId!=null&&v.TrangThai==TrangThaiVong.SanSangBanGiao&&gd.TrangThai==TrangThaiGiaoDich.DangThucHien&&contactRows.Count(x=>x.DaXacNhan)==3&&ships.Count==0,evidence);
    }
}
