using Microsoft.EntityFrameworkCore;
using ReMarket.Application.QuanTri;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.QuanTri;
public sealed partial class QuanTriService
{
    private async Task<List<GiaoDichQuanTriDto>> Trades(IQueryable<GiaoDich> query,CancellationToken ct)
    {
        var result=new List<GiaoDichQuanTriDto>();foreach(var g in await query.ToListAsync(ct))
        {
            var profiles=await (from m in db.ThanhVienGiaoDich.AsNoTracking() join h in db.HoSoCaNhan on m.NguoiDungId equals h.NguoiDungId where m.GiaoDichId==g.Id select new{m.NguoiDungId,m.VaiTroHienThi,h.HoTen}).ToListAsync(ct);
            var version=await db.PhienBanThoaThuan.Where(x=>x.GiaoDichId==g.Id).OrderByDescending(x=>x.SoPhienBan).Select(x=>(Guid?)x.Id).FirstOrDefaultAsync(ct);
            var receipts=await db.XacNhanGiaoNhan.AsNoTracking().Where(x=>x.GiaoDichId==g.Id&&(version==null||x.PhienBanThoaThuanId==version)).ToListAsync(ct);
            var ids=await ProductIds(g.Id,ct);var products=await (from p in db.SanPham.AsNoTracking() join h in db.HoSoCaNhan on p.ChuSoHuuId equals h.NguoiDungId where ids.Contains(p.Id) select new{p.Id,p.Ten,p.GiaTriThamKhao,p.ChuSoHuuId,h.HoTen}).ToListAsync(ct);
            var photos=await db.AnhSanPham.AsNoTracking().Where(x=>ids.Contains(x.SanPhamId)).OrderByDescending(x=>x.LaAnhBia).ThenBy(x=>x.ThuTu).ToListAsync(ct);
            result.Add(new(g.Id,g.Ma,g.Loai.ToString(),g.TrangThai.ToString(),g.NgayTao,g.NgayHoanTat,
                profiles.Select(m=>new ThanhVienQuanTriDto(m.NguoiDungId,m.HoTen,m.VaiTroHienThi,receipts.Any(x=>x.NguoiXacNhanId==m.NguoiDungId&&x.Loai==LoaiXacNhan.DaGiao),receipts.Any(x=>x.NguoiXacNhanId==m.NguoiDungId&&x.Loai==LoaiXacNhan.DaNhan))).ToList(),
                products.Select(p=>new SanPhamQuanTriDto(p.Id,p.Ten,p.GiaTriThamKhao,p.ChuSoHuuId,p.HoTen,photos.FirstOrDefault(x=>x.SanPhamId==p.Id) is {} photo?$"api/admin/giao-dich/{g.Id}/media/product/{photo.Id}":null)).ToList(),
                await db.TienBu.Where(x=>x.GiaoDichId==g.Id&&x.PhienBanThoaThuanId==version).SumAsync(x=>x.SoTien,ct),receipts.Where(x=>x.Loai==LoaiXacNhan.DaNhan).Select(x=>x.NguoiXacNhanId).Distinct().Count(),g.Loai==LoaiGiaoDich.SmartSwap?3:g.Loai==LoaiGiaoDich.TraoDoiMotMot?2:1));
        }return result;
    }
    private async Task<Guid[]> ProductIds(Guid gd,CancellationToken ct)
    {
        var products=await db.ChiTietDonHang.Where(x=>x.DonHang.GiaoDichId==gd).Select(x=>x.SanPhamId).ToListAsync(ct);
        var pair=await db.DeNghiTraoDoi.Where(x=>x.GiaoDichId==gd).Select(x=>new{x.SanPhamGuiId,x.SanPhamNhanId}).SingleOrDefaultAsync(ct);if(pair!=null){products.Add(pair.SanPhamGuiId);products.Add(pair.SanPhamNhanId);}
        products.AddRange(await db.ThanhVienVong.Where(x=>x.GiaoDichId==gd).Select(x=>x.SanPhamId).ToListAsync(ct));return products.Distinct().ToArray();
    }
    public async Task<TrangQuanTri<GiaoDichQuanTriDto>> GiaoDichAsync(Guid actor,string? q,string? type,string? status,int page,CancellationToken ct)
    {
        await Admin(actor,ct);Page(page);var query=db.GiaoDich.AsNoTracking();if(!string.IsNullOrWhiteSpace(q)){var text=q.Trim();query=query.Where(x=>x.Ma.Contains(text));}
        if(!string.IsNullOrEmpty(type)){if(type=="TraoDoi")query=query.Where(x=>x.Loai!=LoaiGiaoDich.MuaBan);else{if(!Enum.TryParse<LoaiGiaoDich>(type,out var value)||!Enum.IsDefined(value))throw new LoiTaiKhoan(400,"Loại giao dịch không hợp lệ.");query=query.Where(x=>x.Loai==value);}}
        if(!string.IsNullOrEmpty(status)){if(!Enum.TryParse<TrangThaiGiaoDich>(status,out var value)||!Enum.IsDefined(value))throw new LoiTaiKhoan(400,"Trạng thái giao dịch không hợp lệ.");query=query.Where(x=>x.TrangThai==value);}
        var total=await query.CountAsync(ct);return PageDto(await Trades(query.OrderByDescending(x=>x.NgayTao).ThenBy(x=>x.Id).Skip((page-1)*12).Take(12),ct),total,page);
    }
    public async Task<ChiTietGiaoDichQuanTriDto> GiaoDichAsync(Guid actor,Guid id,long? before,CancellationToken ct)
    {
        await Admin(actor,ct);var trade=(await Trades(db.GiaoDich.AsNoTracking().Where(x=>x.Id==id),ct)).SingleOrDefault()??throw new LoiTaiKhoan(404,"Không tìm thấy giao dịch.");
        var v=await db.VongSmartSwap.AsNoTracking().SingleOrDefaultAsync(x=>x.GiaoDichId==id,ct);var members=v==null?[]:await db.ThanhVienVong.Where(x=>x.VongSmartSwapId==v.Id).ToListAsync(ct);
        var legs=v==null?[]:await db.ChangTraoDoi.Where(x=>x.VongSmartSwapId==v.Id).ToListAsync(ct);
        var versions=await db.PhienBanThoaThuan.AsNoTracking().Where(x=>x.GiaoDichId==id).OrderByDescending(x=>x.SoPhienBan).ToListAsync(ct);
        var cash=await db.TienBu.AsNoTracking().Where(x=>x.GiaoDichId==id).ToListAsync(ct);var votes=await db.DongYThoaThuan.AsNoTracking().Where(x=>x.GiaoDichId==id).ToListAsync(ct);
        var order=await db.DonHang.AsNoTracking().SingleOrDefaultAsync(x=>x.GiaoDichId==id,ct);
        var ships=await db.VanChuyenMoPhong.AsNoTracking().Where(x=>x.DonHangId!=null&&x.DonHang!.GiaoDichId==id||x.ChangTraoDoiId!=null&&x.ChangTraoDoi!.VongSmartSwap!.GiaoDichId==id).ToListAsync(ct);
        var shipIds=ships.Select(x=>x.Id).ToArray();var history=await db.LichSuVanChuyen.Where(x=>shipIds.Contains(x.VanChuyenMoPhongId)).OrderBy(x=>x.SoThuTu).ToListAsync(ct);
        var chat=db.TinNhan.AsNoTracking().Where(x=>db.CuocTroChuyen.Any(c=>c.Id==x.CuocTroChuyenId&&c.GiaoDichId==id));var totalMessages=await chat.CountAsync(ct);if(before!=null)chat=chat.Where(x=>x.SoThuTu<before);
        var messages=await (from m in chat join h in db.HoSoCaNhan on m.NguoiGuiId equals h.NguoiDungId orderby m.SoThuTu descending select new{m.Id,m.SoThuTu,m.NguoiGuiId,h.HoTen,m.Loai,m.NoiDung,m.NgayTao}).Take(100).ToListAsync(ct);
        var messageIds=messages.Select(x=>x.Id).ToArray();var files=await db.TepTinNhan.Where(x=>messageIds.Contains(x.TinNhanId)).ToListAsync(ct);
        var payment=order==null?null:await db.ThanhToan.AsNoTracking().SingleOrDefaultAsync(x=>x.DonHangId==order.Id,ct);
        var allEvidence=await Evidence(id,ct);
        return new(trade,v?.Id,v?.TrangThai.ToString(),legs.Select(l=>new ChangQuanTriDto(members.Single(m=>m.Id==l.ThanhVienGiaoId).NguoiDungId,members.Single(m=>m.Id==l.ThanhVienNhanId).NguoiDungId,l.SanPhamId)).ToList(),
            versions.Select(p=>new PhienBanQuanTriDto(p.Id,p.SoPhienBan,p.NoiDung,p.TrangThai.ToString(),p.NgayTao,cash.Where(c=>c.PhienBanThoaThuanId==p.Id).Select(c=>new TienBuQuanTriDto(c.NguoiTraId,c.NguoiNhanId,c.SoTien,c.NguoiTraDaXacNhan,c.NguoiNhanDaXacNhan)).ToList(),votes.Where(x=>x.PhienBanThoaThuanId==p.Id).Select(x=>new DongYQuanTriDto(x.NguoiDungId,x.QuyetDinh.ToString())).ToList())).ToList(),
            ships.Select(s=>{var l=legs.SingleOrDefault(x=>x.Id==s.ChangTraoDoiId);return new VanChuyenQuanTriDto(s.Id,l==null?order?.NguoiBanId:members.Single(x=>x.Id==l.ThanhVienGiaoId).NguoiDungId,l==null?order?.NguoiMuaId:members.Single(x=>x.Id==l.ThanhVienNhanId).NguoiDungId,s.MaVanDonTuNhap??s.MaVanDon,s.TrangThai.ToString(),s.NgayNhanDungHang,history.Where(x=>x.VanChuyenMoPhongId==s.Id).Select(x=>new MocQuanTriDto(x.ThoiDiem,x.TrangThai.ToString(),"Mô phỏng",x.MoTa)).ToList());}).ToList(),
            allEvidence,messages.OrderBy(x=>x.SoThuTu).Select(m=>new TinNhanQuanTriDto(m.Id,m.SoThuTu,m.NguoiGuiId,m.HoTen,m.Loai.ToString(),m.NoiDung,m.NgayTao,files.Where(x=>x.TinNhanId==m.Id).Select(x=>new BangChungQuanTriDto(x.Id,"chat",x.TenTep,x.LoaiMime,$"api/admin/giao-dich/{id}/media/chat/{x.Id}",m.NguoiGuiId,x.NgayTao)).ToList())).ToList(),totalMessages,
            await Complaints(db.KhieuNai.AsNoTracking().Where(x=>x.GiaoDichId==id),ct),order?.TrangThai.ToString(),payment?.TrangThai.ToString(),order?.TongTien,versions.FirstOrDefault()?.DiaDiemHen,versions.FirstOrDefault()?.ThoiGianHen);
    }
    private async Task<List<BangChungQuanTriDto>> Evidence(Guid gd,CancellationToken ct)
    {
        var receipts=await db.BangChungGiaoNhan.AsNoTracking().Where(x=>x.XacNhanGiaoNhan.GiaoDichId==gd).Select(x=>new BangChungQuanTriDto(x.Id,"receipt","Bằng chứng giao nhận",x.LoaiMime,$"api/admin/giao-dich/{gd}/media/receipt/{x.Id}",x.NguoiTaiLenId,x.NgayTao)).ToListAsync(ct);
        receipts.AddRange(await db.BangChungKhieuNai.AsNoTracking().Where(x=>x.KhieuNai.GiaoDichId==gd).Select(x=>new BangChungQuanTriDto(x.Id,"complaint",x.MoTa,x.LoaiMime,$"api/admin/giao-dich/{gd}/media/complaint/{x.Id}",x.NguoiTaiLenId,x.NgayTao)).ToListAsync(ct));return receipts;
    }
    public async Task<(string Path,string Mime)> MediaAsync(Guid actor,Guid gd,string kind,Guid file,CancellationToken ct)
    {
        await Admin(actor,ct);if(!await db.GiaoDich.AnyAsync(x=>x.Id==gd,ct))throw new LoiTaiKhoan(404,"Không tìm thấy giao dịch.");
        string? path;string mime="image/jpeg";
        switch(kind){
            case "product":var ids=await ProductIds(gd,ct);path=await db.AnhSanPham.Where(x=>x.Id==file&&ids.Contains(x.SanPhamId)).Select(x=>x.DuongDan).SingleOrDefaultAsync(ct);break;
            case "receipt":var receipt=await db.BangChungGiaoNhan.Where(x=>x.Id==file&&x.XacNhanGiaoNhan.GiaoDichId==gd).SingleOrDefaultAsync(ct);path=receipt?.DuongDan;mime=receipt?.LoaiMime??mime;break;
            case "complaint":var report=await db.BangChungKhieuNai.Where(x=>x.Id==file&&x.KhieuNai.GiaoDichId==gd).SingleOrDefaultAsync(ct);path=report?.DuongDan;mime=report?.LoaiMime??mime;break;
            case "chat":var chat=await db.TepTinNhan.Where(x=>x.Id==file&&db.CuocTroChuyen.Any(c=>c.Id==x.TinNhan.CuocTroChuyenId&&c.GiaoDichId==gd)).SingleOrDefaultAsync(ct);path=chat?.DuongDan;mime=chat?.LoaiMime??mime;break;
            default:throw new LoiTaiKhoan(400,"Loại bằng chứng không hợp lệ.");
        }
        if(path==null)throw new LoiTaiKhoan(404,"Bằng chứng không thuộc giao dịch.");var full=images.DuongDan(path);if(!File.Exists(full))throw new LoiTaiKhoan(404,"Tệp bằng chứng không còn trên máy chủ.");
        var ext=Path.GetExtension(full).ToLowerInvariant();mime=ext==".png"?"image/png":ext is ".jpg" or ".jpeg"?"image/jpeg":ext==".mp4"&&mime=="video/mp4"?"video/mp4":"application/octet-stream";
        Audit(actor,"Xem bằng chứng","GiaoDich",gd,new{kind,file});await db.SaveChangesAsync(ct);return(full,mime);
    }
}
