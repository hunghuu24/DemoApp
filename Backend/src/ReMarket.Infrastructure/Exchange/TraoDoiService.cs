using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SanPham;
using ReMarket.Application.TaiKhoan;
using ReMarket.Application.TraoDoi;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Catalog;
using ReMarket.Infrastructure.Commerce;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.Exchange;

public sealed partial class TraoDoiService(ReMarketDbContext db,KhoaTonKho stock,TinDangService catalog,KhoAnhSanPham images,TimeProvider clock)
{
    private DateTimeOffset Now=>clock.GetUtcNow();
    private static void Key(Guid key){if(key==Guid.Empty)throw new LoiTaiKhoan(400,"Thiếu mã yêu cầu hợp lệ.");}
    private static string Hash(object value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    private static bool Pending(DeNghiTraoDoi offer)=>offer.TrangThai is TrangThaiDeNghi.ChoPhanHoi or TrangThaiDeNghi.DeXuatLai;
    private async Task<DeNghiTraoDoi> CuaToiAsync(Guid id,Guid actor,CancellationToken ct)=>
        await db.DeNghiTraoDoi.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&(x.NguoiGuiId==actor||x.NguoiNhanId==actor),ct)
        ??throw new LoiTaiKhoan(404,"Không tìm thấy đề nghị của bạn.");
    private async Task<(Guid Gui,Guid Nhan)> TinIdsAsync(DeNghiTraoDoi offer,CancellationToken ct)
    {
        async Task<Guid> Legacy(Guid product)=>await db.TinDang.Where(x=>x.SanPhamId==product).OrderByDescending(x=>x.NgayTao).Select(x=>x.Id).FirstAsync(ct);
        return(offer.TinGuiId??await Legacy(offer.SanPhamGuiId),offer.TinNhanId??await Legacy(offer.SanPhamNhanId));
    }
    private async Task<(DeNghiTraoDoi Offer,TinDang Gui,TinDang Nhan)> KhoaAsync(Guid id,Guid? actor,CancellationToken ct)
    {
        var initial=actor is Guid user?await CuaToiAsync(id,user,ct):await db.DeNghiTraoDoi.AsNoTracking().SingleAsync(x=>x.Id==id,ct);
        var ids=await TinIdsAsync(initial,ct);var listings=await stock.NhieuTinAsync([ids.Gui,ids.Nhan],ct);
        var offer=await db.DeNghiTraoDoi.FromSqlInterpolated($"SELECT * FROM dbo.DeNghiTraoDoi WITH (UPDLOCK, ROWLOCK) WHERE Id={id}").SingleAsync(ct);
        await db.Entry(offer).ReloadAsync(ct);
        return(offer,listings[ids.Gui],listings[ids.Nhan]);
    }
    private async Task<PhienBanThoaThuan> HienTaiAsync(Guid gd,CancellationToken ct)=>await db.PhienBanThoaThuan
        .Where(x=>x.GiaoDichId==gd&&x.TrangThai!=TrangThaiThoaThuan.DaThayThe).OrderByDescending(x=>x.SoPhienBan).FirstAsync(ct);
    private async Task KhaDungAsync(TinDang tin,Guid? ownTransaction,CancellationToken ct)
    {
        var ownHold=ownTransaction!=null&&await db.GiuChoSanPham.AnyAsync(x=>x.SanPhamId==tin.SanPhamId&&x.GiaoDichId==ownTransaction&&!x.DaGiaiPhong,ct);
        if(tin.NgayXoa!=null||tin.LoaiTin is not(LoaiTinDang.TraoDoi or LoaiTinDang.CaHai)||
            (tin.TrangThai!=TrangThaiTinDang.DangHienThi&&!(ownHold&&tin.TrangThai==TrangThaiTinDang.DangGiuCho))||
            await db.GiuChoSanPham.AnyAsync(x=>x.SanPhamId==tin.SanPhamId&&!x.DaGiaiPhong&&x.GiaoDichId!=ownTransaction,ct))
            throw new LoiTaiKhoan(409,"Chỉ sử dụng tin trao đổi đã đăng và còn khả dụng.");
        var need=await db.NhuCauTraoDoi.AsNoTracking().SingleOrDefaultAsync(x=>x.TinDangId==tin.Id,ct);
        if(need?.ChoPhepMotMot!=true)throw new LoiTaiKhoan(409,"Tin này không cho phép trao đổi 1–1.");
        if(!await db.NguoiDung.AnyAsync(x=>x.Id==tin.NguoiDangId&&x.TrangThai==TrangThaiNguoiDung.HoatDong,ct))throw new LoiTaiKhoan(409,"Tài khoản đối tác không còn khả dụng.");
        if(tin.TrangThai==TrangThaiTinDang.DangHienThi)await catalog.ChiTietAsync(tin.Id,null,false,ct);
    }
    private async Task BuTienAsync(TinDang gui,TinDang nhan,long amount,Guid payer,CancellationToken ct)
    {
        if(amount<0||amount>1_000_000_000_000)throw new LoiTaiKhoan(400,"Số tiền bù không hợp lệ.");
        if(payer!=gui.NguoiDangId&&payer!=nhan.NguoiDangId)throw new LoiTaiKhoan(400,"Người bù phải thuộc đề nghị.");
        if(amount==0)return;
        var needs=await db.NhuCauTraoDoi.Where(x=>x.TinDangId==gui.Id||x.TinDangId==nhan.Id).ToListAsync(ct);
        var payerTin=payer==gui.NguoiDangId?gui.Id:nhan.Id;
        if(needs.Count!=2||needs.Any(x=>!x.ChoPhepBuTien)||amount>needs.Single(x=>x.TinDangId==payerTin).SoTienBuToiDa)
            throw new LoiTaiKhoan(400,"Tiền bù vượt mức người trả cho phép hoặc có bên không chấp nhận bù tiền.");
    }
    private void PhienMoi(DeNghiTraoDoi offer,int number,Guid actor,long amount,Guid payer,string text,string? location,DateTimeOffset? appointment)
    {
        var now=Now;
        var p=new PhienBanThoaThuan{GiaoDichId=offer.GiaoDichId,SoPhienBan=number,NguoiTaoId=actor,NoiDung=text.Trim(),DiaDiemHen=location?.Trim(),ThoiGianHen=appointment,TrangThai=TrangThaiThoaThuan.DeXuat,NgayTao=now};
        db.Add(p);
        foreach(var member in new[]{offer.NguoiGuiId,offer.NguoiNhanId})db.Add(new DongYThoaThuan{
            GiaoDichId=offer.GiaoDichId,PhienBanThoaThuanId=p.Id,NguoiDungId=member,QuyetDinh=member==actor?QuyetDinhDongY.DongY:QuyetDinhDongY.ChoPhanHoi,NgayPhanHoi=member==actor?now:null,NgayTao=now});
        if(amount>0)db.Add(new TienBu{GiaoDichId=offer.GiaoDichId,PhienBanThoaThuanId=p.Id,NguoiTraId=payer,NguoiNhanId=payer==offer.NguoiGuiId?offer.NguoiNhanId:offer.NguoiGuiId,SoTien=amount,NgayTao=now});
    }
    private void Notify(DeNghiTraoDoi o,string title,string text)
    {
        foreach(var id in new[]{o.NguoiGuiId,o.NguoiNhanId})db.Add(new ThongBao{NguoiDungId=id,GiaoDichId=o.GiaoDichId,Loai=LoaiThongBao.TraoDoi,TieuDe=title,NoiDung=text});
    }
    public async Task<DeNghiTraoDoiDto> TaoAsync(Guid actor,TaoDeNghiRequest r,CancellationToken ct)
    {
        Key(r.MaYeuCau);
        if(!r.DongYQuyDinh||r.LoiNhan.Trim().Length<5||r.TinGuiId==r.TinNhanId)throw new LoiTaiKhoan(400,"Chọn hai món khác nhau, nhập lời nhắn và đồng ý quy định.");
        if(r.ThoiGianHen is {} when&&when<=Now)throw new LoiTaiKhoan(400,"Thời gian hẹn phải ở tương lai.");
        var hash=Hash(new{r.TinGuiId,r.TinNhanId,r.SoTienBu,r.NguoiTraId,Text=r.LoiNhan.Trim(),Location=r.DiaDiemHen?.Trim(),r.ThoiGianHen});
        async Task<DeNghiTraoDoi?> Replay()=>await db.DeNghiTraoDoi.AsNoTracking().SingleOrDefaultAsync(x=>x.NguoiGuiId==actor&&x.MaYeuCau==r.MaYeuCau,ct);
        var old=await Replay();
        if(old!=null){if(old.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã đề nghị đã dùng cho nội dung khác.");return await DocAsync(old.Id,actor,ct);}
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var listings=await stock.NhieuTinAsync([r.TinGuiId,r.TinNhanId],ct);var gui=listings[r.TinGuiId];var nhan=listings[r.TinNhanId];
        old=await Replay();if(old!=null){if(old.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã đề nghị đã dùng cho nội dung khác.");await tx.CommitAsync(ct);return await DocAsync(old.Id,actor,ct);}
        if(gui.NguoiDangId!=actor)throw new LoiTaiKhoan(403,"Chỉ chọn sản phẩm đã đăng của chính bạn.");
        if(nhan.NguoiDangId==actor)throw new LoiTaiKhoan(400,"Không trao đổi với chính mình.");
        KhoaTonKho.PhienBan(gui.PhienBan,r.PhienBanTinGui);KhoaTonKho.PhienBan(nhan.PhienBan,r.PhienBanTinNhan);
        await KhaDungAsync(gui,null,ct);await KhaDungAsync(nhan,null,ct);
        await BuTienAsync(gui,nhan,r.SoTienBu,r.NguoiTraId??actor,ct);
        if(await db.DeNghiTraoDoi.AnyAsync(x=>x.NguoiGuiId==actor&&x.TinGuiId==gui.Id&&x.TinNhanId==nhan.Id&&
            (x.TrangThai==TrangThaiDeNghi.ChoPhanHoi||x.TrangThai==TrangThaiDeNghi.DeXuatLai)&&x.NgayHetHan>Now,ct))throw new LoiTaiKhoan(409,"Đã có đề nghị đang chờ cho cặp món này.");
        var gd=new GiaoDich{Ma="TD"+Guid.NewGuid().ToString("N")[..16].ToUpperInvariant(),Loai=LoaiGiaoDich.TraoDoiMotMot,TrangThai=TrangThaiGiaoDich.ChoDongY,NgayTao=Now};
        var offer=new DeNghiTraoDoi{GiaoDichId=gd.Id,NguoiGuiId=actor,NguoiNhanId=nhan.NguoiDangId,SanPhamGuiId=gui.SanPhamId,SanPhamNhanId=nhan.SanPhamId,TinGuiId=gui.Id,TinNhanId=nhan.Id,TrangThai=TrangThaiDeNghi.ChoPhanHoi,NgayTao=Now,NgayHetHan=Now.AddHours(24),MaYeuCau=r.MaYeuCau,BamYeuCau=hash};
        db.Add(gd);db.AddRange(new ThanhVienGiaoDich{GiaoDichId=gd.Id,NguoiDungId=actor,VaiTroHienThi="NguoiGui"},new ThanhVienGiaoDich{GiaoDichId=gd.Id,NguoiDungId=nhan.NguoiDangId,VaiTroHienThi="NguoiNhan"});db.Add(offer);
        PhienMoi(offer,1,actor,r.SoTienBu,r.NguoiTraId??actor,r.LoiNhan,r.DiaDiemHen,r.ThoiGianHen);
        Notify(offer,"Đề nghị trao đổi mới","Đối tác cần phản hồi trước hạn. Chưa có bàn giao hoặc chuyển tiền.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(offer.Id,actor,ct);
    }
    public async Task<TrangTinDto> MonCuaToiAsync(Guid actor,string? query,int page,CancellationToken ct)
    {
        var filter=new BoLocTin{LoaiTin="TraoDoi",ChoPhepMotMot=true,TuKhoa=query,Trang=page,KichThuocTrang=10};
        return await catalog.TimAsync(filter,ct,actor);
    }
}
