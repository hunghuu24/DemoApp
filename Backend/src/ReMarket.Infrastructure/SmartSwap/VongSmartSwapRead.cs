using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Application.SanPham;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed partial class VongSmartSwapService
{
    public async Task<ChiTietVongSmartSwapDto> DocAsync(Guid id,Guid actor,CancellationToken ct,bool expire=true)
    {
        var v=await CuaToiAsync(id,actor,ct);
        if(expire&&v.NguoiKhoiTaoId!=null&&v.TrangThai==TrangThaiVong.ChoDongY&&v.NgayHetHan<=Now)
        {await XuLyHetHanAsync(id,actor,ct);v=await CuaToiAsync(id,actor,ct);}
        var members=await ThanhVienAsync(id,ct);var users=members.Select(x=>x.Member.NguoiDungId).ToArray();var profiles=await HoSoAsync(users,ct);
        string Name(Guid user)=>profiles.FirstOrDefault(x=>x.NguoiDungId==user)?.HoTen??"Thành viên ReMarket";
        var gd=await db.GiaoDich.AsNoTracking().SingleAsync(x=>x.Id==v.GiaoDichId,ct);
        var legs=await db.ChangTraoDoi.AsNoTracking().Where(x=>x.VongSmartSwapId==id).ToListAsync(ct);
        var productIds=members.Select(x=>x.Member.SanPhamId).ToArray();
        var products=await db.SanPham.AsNoTracking().Where(x=>productIds.Contains(x.Id)).ToListAsync(ct);
        var tinIds=members.Select(x=>x.TinId).ToArray();var tins=await db.TinDang.AsNoTracking().Where(x=>tinIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var needs=await db.NhuCauTraoDoi.AsNoTracking().Where(x=>tinIds.Contains(x.TinDangId)).ToDictionaryAsync(x=>x.TinDangId,ct);
        var photoProducts=await db.AnhSanPham.Where(x=>productIds.Contains(x.SanPhamId)).Select(x=>x.SanPhamId).Distinct().ToListAsync(ct);
        MonSmartSwapDto Product(ThanhVienVong m,Guid tinId)
        {
            var p=products.Single(x=>x.Id==m.SanPhamId);var t=tins[tinId];return new(t.Id,p.Id,m.NguoiDungId,p.Ten,p.GiaTriThamKhao,p.TinhTrang.ToString(),t.TinhThanh,Convert.ToBase64String(t.PhienBan));
        }
        var memberRows=members.Select(x=>new ThanhVienSmartSwapDto(x.Member.Id,x.Member.ViTri,x.Member.NguoiDungId,Name(x.Member.NguoiDungId),Product(x.Member,x.TinId),
            members.Single(a=>a.Member.Id==legs.Single(l=>l.ThanhVienNhanId==x.Member.Id).ThanhVienGiaoId).Member.NguoiDungId,
            members.Single(a=>a.Member.Id==legs.Single(l=>l.ThanhVienGiaoId==x.Member.Id).ThanhVienNhanId).Member.NguoiDungId,
            legs.Single(l=>l.ThanhVienNhanId==x.Member.Id).SanPhamId,products.Single(p=>p.Id==x.Member.SanPhamId).MoTa,
            needs.TryGetValue(x.TinId,out var n)?new NhuCauTraoDoiDto(n.MoTaMongMuon,n.ChoPhepMotMot,n.ChoPhepSmartSwap,n.ChoPhepBuTien,n.SoTienBuToiDa,n.GhiChu,
                new(n.DanhMucMongMuonId,n.TuKhoaMongMuon,n.TinhTrangToiThieu?.ToString(),n.GiaTriMongMuonTu,n.GiaTriMongMuonDen)):null,
            photoProducts.Contains(x.Member.SanPhamId)?$"api/smart-swaps/{id}/products/{x.Member.SanPhamId}/image":null)).ToList();
        var legRows=members.Select(x=>{var l=legs.Single(a=>a.ThanhVienGiaoId==x.Member.Id);return new ChangVongSmartSwapDto(x.Member.NguoiDungId,
            members.Single(a=>a.Member.Id==l.ThanhVienNhanId).Member.NguoiDungId,Product(x.Member,x.TinId));}).ToList();
        var agreements=await db.PhienBanThoaThuan.AsNoTracking().Where(x=>x.GiaoDichId==v.GiaoDichId).OrderBy(x=>x.SoPhienBan).ToListAsync(ct);
        var ids=agreements.Select(x=>x.Id).ToArray();var votes=await db.DongYThoaThuan.AsNoTracking().Where(x=>ids.Contains(x.PhienBanThoaThuanId)).ToListAsync(ct);
        var money=await db.TienBu.AsNoTracking().Where(x=>ids.Contains(x.PhienBanThoaThuanId)).ToListAsync(ct);
        var messages=await db.TinNhan.AsNoTracking().Where(x=>x.PhienBanThoaThuanId!=null&&ids.Contains(x.PhienBanThoaThuanId.Value)).ToListAsync(ct);
        var history=agreements.Select(p=>{var rows=money.Where(x=>x.PhienBanThoaThuanId==p.Id).OrderBy(x=>x.NguoiTraId).ThenBy(x=>x.NguoiNhanId)
            .Select(x=>new KhoanBuSmartSwapDto(x.NguoiTraId,x.NguoiNhanId,x.SoTien)).ToList();
            return new PhuongAnSmartSwapDto(p.Id,p.SoPhienBan,p.NguoiTaoId,p.TrangThai.ToString(),p.NoiDung,p.NgayTao,p.MaYeuCau,
                messages.SingleOrDefault(x=>x.PhienBanThoaThuanId==p.Id)?.Id,rows,rows.Sum(x=>x.SoTien),rows.Sum(x=>x.SoTien),
                votes.Where(x=>x.PhienBanThoaThuanId==p.Id).OrderBy(x=>x.NguoiDungId).Select(x=>new DongYSmartSwapDto(x.NguoiDungId,Name(x.NguoiDungId),x.QuyetDinh.ToString(),x.NgayPhanHoi)).ToList());}).ToList();
        var current=history.LastOrDefault();var handover=await DaBanGiaoAsync(v.GiaoDichId,ct);
        var managed=v.NguoiKhoiTaoId!=null;var open=managed&&v.TrangThai==TrangThaiVong.ChoDongY&&gd.TrangThai==TrangThaiGiaoDich.ChoDongY&&v.NgayHetHan>Now;
        var active=managed&&(open||v.TrangThai==TrangThaiVong.SanSangBanGiao&&gd.TrangThai==TrangThaiGiaoDich.DangThucHien);
        var chatId=await db.CuocTroChuyen.Where(x=>x.GiaoDichId==v.GiaoDichId).Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct);
        return new(v.Id,v.GiaoDichId,gd.Ma,Convert.ToBase64String(v.PhienBan),v.NguoiKhoiTaoId,v.TrangThai.ToString(),gd.TrangThai.ToString(),v.NgayHetHan,v.NgayChapNhan,
            v.MaKetThuc,v.LyDoKetThuc,chatId,memberRows,legRows,current,history,
            open&&current?.DongY.Any(x=>x.NguoiDungId==actor&&x.QuyetDinh=="ChoPhanHoi")==true,
            active&&!handover,active&&!handover,active&&v.TrangThai==TrangThaiVong.SanSangBanGiao&&current?.TrangThai=="DaChot"&&current.DongY.Count(x=>x.QuyetDinh=="DongY")==3,
            await TienDoAsync(v.GiaoDichId,current?.Id,v.TrangThai,ct));
    }
    public async Task<TrangVongSmartSwapDto> DanhSachAsync(Guid actor,string tab,int page,CancellationToken ct)
    {
        if(tab is not("tatca" or "gui" or "nhan" or "chodongy" or "chinhthuc" or "dahuy" or "tamdung" or "hoantat"))throw new LoiTaiKhoan(400,"Bộ lọc vòng không hợp lệ.");
        var query=db.VongSmartSwap.AsNoTracking().Where(x=>db.ThanhVienVong.Any(m=>m.VongSmartSwapId==x.Id&&m.NguoiDungId==actor));
        var expired=await query.Where(x=>x.NguoiKhoiTaoId!=null&&x.TrangThai==TrangThaiVong.ChoDongY&&x.GiaoDich.TrangThai==TrangThaiGiaoDich.ChoDongY&&x.NgayHetHan<=Now).Take(20).Select(x=>x.Id).ToListAsync(ct);
        foreach(var id in expired)await XuLyHetHanAsync(id,actor,ct);
        query=tab switch{"gui"=>query.Where(x=>x.NguoiKhoiTaoId==actor),"nhan"=>query.Where(x=>x.NguoiKhoiTaoId!=actor),
            "chodongy"=>query.Where(x=>x.TrangThai==TrangThaiVong.ChoDongY&&x.GiaoDich.TrangThai==TrangThaiGiaoDich.ChoDongY),
            "chinhthuc"=>query.Where(x=>(x.TrangThai==TrangThaiVong.SanSangBanGiao||x.TrangThai==TrangThaiVong.DangBanGiao)&&x.GiaoDich.TrangThai==TrangThaiGiaoDich.DangThucHien),
            "tamdung"=>query.Where(x=>x.TrangThai==TrangThaiVong.TamDung),"hoantat"=>query.Where(x=>x.TrangThai==TrangThaiVong.HoanTat),
            "dahuy"=>query.Where(x=>x.TrangThai==TrangThaiVong.DaHuy),_=>query};
        var count=await query.CountAsync(ct);var ids=await query.OrderByDescending(x=>x.NgayTao).ThenBy(x=>x.Id).Skip((page-1)*10).Take(10).Select(x=>x.Id).ToListAsync(ct);
        var rows=new List<ChiTietVongSmartSwapDto>();foreach(var id in ids)rows.Add(await DocAsync(id,actor,ct));return new(rows,count,page,(int)Math.Ceiling(count/10d));
    }
    public async Task<Guid> ChatAsync(Guid id,Guid actor,CancellationToken ct)
    {await CuaToiAsync(id,actor,ct);return await db.CuocTroChuyen.Where(x=>x.GiaoDichId==db.VongSmartSwap.Where(v=>v.Id==id).Select(v=>v.GiaoDichId).Single()).Select(x=>x.Id).SingleAsync(ct);}
    public async Task<string> AnhAsync(Guid id,Guid product,Guid actor,CancellationToken ct)
    {
        await CuaToiAsync(id,actor,ct);
        if(!await db.ThanhVienVong.AnyAsync(x=>x.VongSmartSwapId==id&&x.SanPhamId==product,ct))throw new LoiTaiKhoan(404,"Ảnh không thuộc vòng này.");
        var file=await db.AnhSanPham.Where(x=>x.SanPhamId==product).OrderByDescending(x=>x.LaAnhBia).ThenBy(x=>x.ThuTu).Select(x=>x.DuongDan).FirstOrDefaultAsync(ct)
            ??throw new LoiTaiKhoan(404,"Không tìm thấy ảnh sản phẩm.");
        var path=images.DuongDan(file);if(!File.Exists(path))throw new LoiTaiKhoan(404,"Không tìm thấy ảnh sản phẩm.");return path;
    }
    public Task<List<ThongBaoSmartSwapDto>> ThongBaoAsync(Guid actor,bool unread,int page,CancellationToken ct)=>
        (from n in db.ThongBao.AsNoTracking() join v in db.VongSmartSwap.AsNoTracking() on n.GiaoDichId equals v.GiaoDichId
         where n.NguoiDungId==actor&&(!unread||!n.DaDoc)&&db.ThanhVienVong.Any(m=>m.VongSmartSwapId==v.Id&&m.NguoiDungId==actor)
         orderby n.NgayTao descending,n.Id select new ThongBaoSmartSwapDto(n.Id,v.Id,n.TieuDe,n.NoiDung,n.DaDoc,n.NgayTao)).Skip((page-1)*20).Take(20).ToListAsync(ct);
    public async Task DocThongBaoAsync(Guid id,Guid actor,CancellationToken ct)
    {
        var count=await db.ThongBao.Where(n=>n.Id==id&&n.NguoiDungId==actor&&db.VongSmartSwap.Any(v=>v.GiaoDichId==n.GiaoDichId&&db.ThanhVienVong.Any(m=>m.VongSmartSwapId==v.Id&&m.NguoiDungId==actor)))
            .ExecuteUpdateAsync(x=>x.SetProperty(n=>n.DaDoc,true),ct);
        if(count==0)throw new LoiTaiKhoan(404,"Không tìm thấy thông báo Smart Swap của bạn.");
    }
}
