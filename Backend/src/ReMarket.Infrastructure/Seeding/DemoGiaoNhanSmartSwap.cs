using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SanPham;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Catalog;
using ReMarket.Infrastructure.Persistence;
using ReMarket.Infrastructure.SmartSwap;
using SkiaSharp;

namespace ReMarket.Infrastructure.Seeding;

/// <summary>Explicit Development fixture built through real services. Leaves one real circle at2/3.</summary>
public sealed class DemoGiaoNhanSmartSwap(ReMarketDbContext db,ITaiKhoanService accounts,IPasswordHasher<NguoiDung> hasher,
    TinDangService catalog,TimSmartSwapService search,VongSmartSwapService swaps)
{
    private const string Marker="DemoGiaoNhanSmartSwapV1";
    public static readonly string[] Emails=["swap-ship-a@remarket.example.test","swap-ship-b@remarket.example.test","swap-ship-c@remarket.example.test"];
    public async Task<Guid> KhoiTaoAsync(CancellationToken ct=default)
    {
        if((await db.Database.GetPendingMigrationsAsync(ct)).Any())throw new InvalidOperationException("Apply migrations before seeding delivery demo.");
        await using var session=new SqlConnection(db.Database.GetConnectionString());await session.OpenAsync(ct);
        await using(var cmd=session.CreateCommand())
        {cmd.CommandText="DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource='ReMarket:SmartDeliveryDemo:v1',@LockMode='Exclusive',@LockOwner='Session',@LockTimeout=10000; IF @r<0 THROW 51000,'Demo lock unavailable',1;";await cmd.ExecuteNonQueryAsync(ct);}
        try
        {
            var marker=await db.NhatKyQuanTri.AsNoTracking().SingleOrDefaultAsync(x=>x.HanhDong==Marker,ct);
            if(marker!=null)return JsonSerializer.Deserialize<Dictionary<string,Guid>>(marker.ChiTietJson!)!["vongId"];
            var password=Environment.GetEnvironmentVariable("REMARKET_DEMO_PASSWORD")??"ReMarketDemo!2026";
            var userIds=new List<Guid>();
            for(var i=0;i<3;i++)
            {
                var email=Emails[i];var existing=await db.NguoiDung.SingleOrDefaultAsync(x=>x.Email==email,ct);
                if(existing==null)
                {
                    var profile=await accounts.DangKyAsync(new("Smart Delivery Demo "+(char)('A'+i),email,"098880000"+(i+1),password,password,true),ct);userIds.Add(profile.Id);
                }
                else
                {
                    if(existing.TrangThai!=TrangThaiNguoiDung.HoatDong||hasher.VerifyHashedPassword(existing,existing.MatKhauBam,password)==PasswordVerificationResult.Failed)
                        throw new InvalidOperationException("Dedicated demo account already exists with changed credentials/state. No password or user data was reset.");
                    userIds.Add(existing.Id);
                }
            }
            var admin=await db.NguoiDungVaiTro.Where(x=>x.VaiTro.Ma=="ADMIN"&&x.NguoiDung.TrangThai==TrangThaiNguoiDung.HoatDong).Select(x=>x.NguoiDungId).FirstAsync(ct);
            var category=await db.DanhMuc.SingleOrDefaultAsync(x=>x.Ma=="SMART_DELIVERY_DEMO",ct);
            if(category==null){category=new DanhMuc{Ma="SMART_DELIVERY_DEMO",Ten="Demo giao nhận Smart Swap",DangHienThi=true};db.Add(category);await db.SaveChangesAsync(ct);}
            var requestKey=DuLieuMau.Ma("smart-delivery-invite-v1");
            var existingCircle=await db.VongSmartSwap.AsNoTracking().SingleOrDefaultAsync(x=>x.NguoiKhoiTaoId==userIds[0]&&x.MaYeuCau==requestKey,ct);
            ChiTietVongSmartSwapDto circle;
            if(existingCircle==null)
            {
                string[] names=["iPhone SmartDeliveryDemo","Laptop SmartDeliveryDemo","iPad SmartDeliveryDemo"];
                string[] wants=["Laptop SmartDeliveryDemo","iPad SmartDeliveryDemo","iPhone SmartDeliveryDemo"];
                var listings=new List<TinDangDto>();byte[] png;
                using(var bitmap=new SKBitmap(160,100)){bitmap.Erase(SKColors.ForestGreen);using var data=bitmap.Encode(SKEncodedImageFormat.Png,100);png=data.ToArray();}
                for(var i=0;i<3;i++)
                {
                    var owner=userIds[i];var title=names[i];
                    var oldId=await db.TinDang.Where(x=>x.NguoiDangId==owner&&x.SanPham.Ten==title&&x.NgayXoa==null).Select(x=>(Guid?)x.Id).SingleOrDefaultAsync(ct);
                    var tin=oldId==null?await catalog.TaoAsync(owner,new(title,category.Id,10_000_000,"NhuMoi","Sản phẩm demo, ảnh minh họa; chỉ vận chuyển mô phỏng.","Hà Nội","Địa chỉ demo Hà Nội",null,
                        new("Muốn nhận "+wants[i],true,true,true,2_000_000,null,new(category.Id,wants[i],"DaSuDungTot",0,20_000_000))),ct):await catalog.ChiTietAsync(oldId.Value,owner,true,ct);
                    if(tin.Anh.Count==0){using var stream=new MemoryStream(png);tin=await catalog.ThemAnhAsync(tin.Id,owner,tin.PhienBan,stream,ct);}
                    if(tin.TrangThai=="BanNhap")tin=await catalog.ChuyenAsync(tin.Id,owner,tin.PhienBan,"gui-duyet",ct);
                    if(tin.TrangThai=="ChoDuyet")tin=await catalog.DuyetAsync(tin.Id,admin,new(tin.PhienBan,true,null),ct);
                    if(tin.TrangThai!="DangHienThi")throw new InvalidOperationException("Demo listing was changed; existing history was preserved.");
                    listings.Add(tin);
                }
                var found=(await search.GoiYBaTinAsync(listings.Select(x=>x.Id).ToArray(),ct)).Single();
                var preview=await swaps.XemAsync(userIds[0],new(found.MaDeXuat),ct);
                circle=await swaps.MoiAsync(userIds[0],new(requestKey,found.MaDeXuat,preview.BamPhuongAn,"Demo ba tài khoản, mỗi người gửi một món và tự xác nhận nhận hàng.",true),ct);
            }
            else circle=await swaps.DocAsync(existingCircle.Id,userIds[0],ct);
            if(circle.TrangThai=="ChoDongY")
                foreach(var actor in userIds)circle=await swaps.PhanHoiAsync(circle.Id,actor,new(circle.PhuongAnHienTai!.Id,true,true),ct);
            if(circle.TrangThai is not("SanSangBanGiao" or "DangBanGiao"))throw new InvalidOperationException("Demo circle has been changed/ended; no state reset was performed.");
            var delivery=await swaps.GiaoNhanAsync(circle.Id,userIds[0],ct);
            if(delivery.VanChuyen.Count==0)
            {
                for(var i=0;i<3;i++)
                {
                    var contact=delivery.LienHe.Single(x=>x.NguoiDungId==userIds[i]);
                    if(contact.DaXacNhan)continue;
                    // Re-read own version only; another member's personal contact is redacted.
                    var own=(await swaps.GiaoNhanAsync(circle.Id,userIds[i],ct)).LienHe.Single(x=>x.NguoiDungId==userIds[i]);
                    delivery=await swaps.LienHeAsync(circle.Id,userIds[i],new(DuLieuMau.Ma("smart-delivery-contact-"+i),circle.PhuongAnHienTai!.Id,
                        "Smart Delivery Demo "+(char)('A'+i),"098880000"+(i+1),$"{10+i} Đường Gửi Demo, Hà Nội",$"{20+i} Đường Nhận Demo, Hà Nội",true,own.PhienBan),ct);
                }
                delivery=await swaps.TaoVanChuyenAsync(circle.Id,userIds[0],new(circle.PhuongAnHienTai!.Id),ct);
            }
            foreach(var row in delivery.VanChuyen.ToArray())
            {
                var ship=row;
                if(ship.MaVanDon==null)
                {
                    var after=await swaps.MaVanDonAsync(circle.Id,ship.Id,ship.NguoiGuiId,new(ship.PhienBan,"DEMO-SS-"+ship.Id.ToString("N")[..12]),ct);ship=after.VanChuyen.Single(x=>x.Id==ship.Id);
                }
                while(ship.TrangThai!="DaGiao")
                {
                    var next=ship.TrangThai switch{"ChoGui"=>"DaGui","DaGui"=>"DangVanChuyen","DangVanChuyen"=>"DaGiao",_=>throw new InvalidOperationException("Unknown demo shipment state.")};
                    var after=await swaps.ChuyenVanChuyenAsync(circle.Id,ship.Id,ship.NguoiGuiId,new(ship.PhienBan,next,true),ct);ship=after.VanChuyen.Single(x=>x.Id==ship.Id);
                }
            }
            for(var i=0;i<2;i++)
            {
                var actor=userIds[i];delivery=await swaps.GiaoNhanAsync(circle.Id,actor,ct);var incoming=delivery.VanChuyen.Single(x=>x.NguoiNhanId==actor);
                if(incoming.NgayNhanDungHang==null)await swaps.NhanDungHangAsync(circle.Id,incoming.Id,actor,new(circle.PhuongAnHienTai!.Id,true,true,true),ct);
            }
            db.Add(new NhatKyQuanTri{NguoiThucHienId=admin,HanhDong=Marker,LoaiDoiTuong="SmartSwap",DoiTuongId=circle.Id,ChiTietJson=JsonSerializer.Serialize(new{vongId=circle.Id})});
            await db.SaveChangesAsync(ct);return circle.Id;
        }
        finally
        {await using var cmd=session.CreateCommand();cmd.CommandText="EXEC sys.sp_releaseapplock @Resource='ReMarket:SmartDeliveryDemo:v1',@LockOwner='Session';";await cmd.ExecuteNonQueryAsync(CancellationToken.None);}
    }
}
