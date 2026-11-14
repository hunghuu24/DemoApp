using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Commerce;
using ReMarket.Infrastructure.Catalog;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed partial class VongSmartSwapService(ReMarketDbContext db,KhoaTonKho stock,TimSmartSwapService matching,KhoAnhSanPham images,TimeProvider clock)
{
    private DateTimeOffset Now=>clock.GetUtcNow();
    private static string Hash(object value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    private static Guid[] DocMa(string key)
    {
        var parts=key.Split('.');
        if(parts.Length!=3||parts.Any(x=>!Guid.TryParseExact(x,"N",out var id)||id==Guid.Empty))throw new LoiTaiKhoan(400,"Mã vòng đề xuất không hợp lệ.");
        var ids=parts.Select(Guid.Parse).ToArray();
        if(ids.Distinct().Count()!=3)throw new LoiTaiKhoan(400,"Vòng phải có ba tin khác nhau.");
        return ids;
    }
    private static void Key(Guid key){if(key==Guid.Empty)throw new LoiTaiKhoan(400,"Thiếu mã yêu cầu hợp lệ.");}
    private Task<List<HoSoThanhVienSmartSwapDto>> HoSoAsync(IEnumerable<Guid> ids,CancellationToken ct)=>db.HoSoCaNhan.AsNoTracking()
        .Where(x=>ids.Contains(x.NguoiDungId)).OrderBy(x=>x.NguoiDungId).Select(x=>new HoSoThanhVienSmartSwapDto(x.NguoiDungId,x.HoTen)).ToListAsync(ct);
    public async Task<XemVongSmartSwapDto> XemAsync(Guid actor,XemVongSmartSwapRequest r,CancellationToken ct)
    {
        var key=r.MaDeXuat.ToLowerInvariant();var v=await matching.KiemTraVongAsync(DocMa(key),key,null,ct);
        return new(v,await HoSoAsync(v.ThanhVien.Select(x=>x.NguoiDungId),ct),Hash(v),v.ThanhVien.Any(x=>x.NguoiDungId==actor));
    }
    private async Task AppLockAsync(Guid actor,Guid key,CancellationToken ct)
    {
        var resource="smart-swap:create:"+actor.ToString("N")+":"+key.ToString("N");
        // Transaction-owned key lock also serializes retries with different/disjoint product sets.
        try{await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={resource},@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=5000; IF @r<0 THROW 51020,'Request lock unavailable',1;",ct);}
        catch(SqlException ex)when(ex.Number==51020){throw new LoiTaiKhoan(409,"Yêu cầu đang được xử lý; hãy thử lại cùng mã yêu cầu.");}
    }
    private async Task<VongSmartSwap> CuaToiAsync(Guid id,Guid actor,CancellationToken ct)=>
        await db.VongSmartSwap.AsNoTracking().SingleOrDefaultAsync(v=>v.Id==id&&db.ThanhVienVong.Any(m=>m.VongSmartSwapId==v.Id&&m.NguoiDungId==actor),ct)
        ??throw new LoiTaiKhoan(404,"Không tìm thấy vòng Smart Swap của bạn.");
    private async Task<List<(ThanhVienVong Member,Guid TinId)>> ThanhVienAsync(Guid id,CancellationToken ct)
    {
        var members=await db.ThanhVienVong.AsNoTracking().Where(x=>x.VongSmartSwapId==id).OrderBy(x=>x.ViTri).ToListAsync(ct);
        var result=new List<(ThanhVienVong,Guid)>();
        foreach(var m in members)
        {
            var tin=m.TinDangId??await db.TinDang.Where(x=>x.SanPhamId==m.SanPhamId).OrderByDescending(x=>x.NgayTao).Select(x=>x.Id).FirstAsync(ct);
            result.Add((m,tin));
        }
        return result;
    }
    private async Task<(VongSmartSwap Vong,Dictionary<Guid,TinDang> Tin)> KhoaAsync(Guid id,Guid? actor,CancellationToken ct)
    {
        if(actor is Guid user)await CuaToiAsync(id,user,ct);
        var members=await ThanhVienAsync(id,ct);
        var tins=await stock.NhieuTinAsync(members.Select(x=>x.TinId),ct);
        var circle=await db.VongSmartSwap.FromSqlInterpolated($"SELECT * FROM dbo.VongSmartSwap WITH (UPDLOCK,ROWLOCK) WHERE Id={id}").SingleAsync(ct);
        await db.Entry(circle).ReloadAsync(ct);
        return(circle,tins);
    }
    private Task<PhienBanThoaThuan> HienTaiAsync(Guid gd,CancellationToken ct)=>db.PhienBanThoaThuan.Where(x=>x.GiaoDichId==gd)
        .OrderByDescending(x=>x.SoPhienBan).FirstAsync(ct);
    private async Task<CuocTroChuyen> KhoaChatAsync(Guid gd,CancellationToken ct)=>await db.CuocTroChuyen
        .FromSqlInterpolated($"SELECT * FROM dbo.CuocTroChuyen WITH (UPDLOCK,ROWLOCK) WHERE GiaoDichId={gd}").SingleAsync(ct);
    private void Notify(IEnumerable<Guid> users,VongSmartSwap v,string title,string text)
    { foreach(var id in users.Distinct())db.Add(new ThongBao{NguoiDungId=id,GiaoDichId=v.GiaoDichId,Loai=LoaiThongBao.TraoDoi,TieuDe=title,NoiDung=text,NgayTao=Now}); }
    private void Event(CuocTroChuyen chat,Guid actor,string text,Guid? version=null)=>db.Add(new TinNhan{
        CuocTroChuyenId=chat.Id,NguoiGuiId=actor,MaPhiaClient=Guid.NewGuid(),Loai=LoaiTinNhan.HeThong,NoiDung=text,PhienBanThoaThuanId=version,NgayTao=Now});
    private void TaoPhien(VongSmartSwap v,Guid actor,int number,IEnumerable<Guid> members,IEnumerable<KhoanBuSmartSwapDto> money,
        string text,bool creatorAccepted,CuocTroChuyen chat,Guid? requestKey=null,string? hash=null)
    {
        var p=new PhienBanThoaThuan{GiaoDichId=v.GiaoDichId,NguoiTaoId=actor,SoPhienBan=number,NoiDung=text.Trim(),
            TrangThai=TrangThaiThoaThuan.DeXuat,MaYeuCau=requestKey,BamYeuCau=hash,NgayTao=Now};db.Add(p);
        foreach(var id in members)db.Add(new DongYThoaThuan{GiaoDichId=v.GiaoDichId,PhienBanThoaThuanId=p.Id,NguoiDungId=id,
            QuyetDinh=creatorAccepted&&id==actor?QuyetDinhDongY.DongY:QuyetDinhDongY.ChoPhanHoi,NgayPhanHoi=creatorAccepted&&id==actor?Now:null,NgayTao=Now});
        foreach(var m in money)db.Add(new TienBu{GiaoDichId=v.GiaoDichId,PhienBanThoaThuanId=p.Id,NguoiTraId=m.NguoiTraId,NguoiNhanId=m.NguoiNhanId,SoTien=m.SoTien,NgayTao=Now});
        Event(chat,actor,$"Phương án tiền bù phiên bản {number}: {text.Trim()}",p.Id);
    }
    public async Task<ChiTietVongSmartSwapDto> MoiAsync(Guid actor,GuiLoiMoiSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.MaYeuCau);if(!r.DongYPhuongAn)throw new LoiTaiKhoan(400,"Người khởi tạo cần xác nhận phương án trước khi gửi lời mời.");
        if(r.LoiNhan.Trim().Length<5)throw new LoiTaiKhoan(400,"Lời nhắn cần ít nhất5 ký tự.");
        var key=r.MaDeXuat.ToLowerInvariant();var ids=DocMa(key);
        var hash=Hash(new{Key=key,Plan=r.BamPhuongAn.ToUpperInvariant(),Text=r.LoiNhan.Trim()});
        await using var tx=await db.Database.BeginTransactionAsync(ct);await AppLockAsync(actor,r.MaYeuCau,ct);
        var old=await db.VongSmartSwap.AsNoTracking().SingleOrDefaultAsync(x=>x.NguoiKhoiTaoId==actor&&x.MaYeuCau==r.MaYeuCau,ct);
        if(old!=null){if(old.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã lời mời đã dùng cho nội dung khác.");await tx.CommitAsync(ct);return await DocAsync(old.Id,actor,ct);}
        await stock.NhieuTinAsync(ids,ct);
        var proposal=await matching.KiemTraVongAsync(ids,key,null,ct);
        if(proposal.ThanhVien.All(x=>x.NguoiDungId!=actor))throw new LoiTaiKhoan(403,"Bạn phải có một sản phẩm trong vòng để khởi tạo.");
        if(!string.Equals(Hash(proposal),r.BamPhuongAn,StringComparison.OrdinalIgnoreCase))throw new LoiTaiKhoan(409,"Phương án đã thay đổi; hãy xem lại trước khi gửi.");
        if(await db.VongSmartSwap.AnyAsync(x=>x.MaDeXuat==key&&x.TrangThai==TrangThaiVong.ChoDongY,ct))throw new LoiTaiKhoan(409,"Đã có lời mời đang chờ cho vòng này.");
        var gd=new GiaoDich{Ma="SS"+Guid.NewGuid().ToString("N")[..18].ToUpperInvariant(),Loai=LoaiGiaoDich.SmartSwap,TrangThai=TrangThaiGiaoDich.ChoDongY,NgayTao=Now};
        var users=proposal.ThanhVien.Select(x=>x.NguoiDungId).ToArray();
        db.Add(gd);foreach(var id in users)db.Add(new ThanhVienGiaoDich{GiaoDichId=gd.Id,NguoiDungId=id,VaiTroHienThi="ThanhVienSmartSwap",NgayTao=Now});
        var v=new VongSmartSwap{GiaoDichId=gd.Id,NguoiKhoiTaoId=actor,MaYeuCau=r.MaYeuCau,BamYeuCau=hash,MaDeXuat=key,
            TrangThai=TrangThaiVong.ChoDongY,NgayTao=Now,NgayHetHan=Now.AddHours(24)};db.Add(v);
        var members=proposal.Chang.Select((leg,index)=>new ThanhVienVong{VongSmartSwapId=v.Id,GiaoDichId=gd.Id,
            NguoiDungId=leg.NguoiGiaoId,SanPhamId=leg.SanPham.SanPhamId,TinDangId=leg.SanPham.TinDangId,ViTri=index+1,NgayTao=Now}).ToArray();
        db.AddRange(members);
        foreach(var leg in proposal.Chang)db.Add(new ChangTraoDoi{VongSmartSwapId=v.Id,ThanhVienGiaoId=members.Single(x=>x.NguoiDungId==leg.NguoiGiaoId).Id,
            ThanhVienNhanId=members.Single(x=>x.NguoiDungId==leg.NguoiNhanId).Id,SanPhamId=leg.SanPham.SanPhamId,NgayTao=Now});
        var chat=new CuocTroChuyen{GiaoDichId=gd.Id,Loai=LoaiTroChuyen.NhomSmartSwap,TieuDe="Smart Swap · "+gd.Ma,NgayTao=Now};db.Add(chat);
        foreach(var id in users)db.Add(new ThanhVienTroChuyen{CuocTroChuyenId=chat.Id,NguoiDungId=id,NgayTao=Now});
        TaoPhien(v,actor,1,users,proposal.TienBu,r.LoiNhan,true,chat);
        Notify(users.Where(x=>x!=actor),v,"Lời mời Smart Swap mới","Xem đủ ba món, chiều giao và tiền bù phiên bản1. Chưa có sản phẩm được giữ.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(v.Id,actor,ct);
    }
}
