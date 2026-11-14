using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReMarket.Api.Controllers;
using ReMarket.Application.MuaBan;
using ReMarket.Application.SanPham;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;
using ReMarket.Infrastructure.Seeding;
using SkiaSharp;

namespace ReMarket.DatabaseChecks;

internal static class KiemTraGiaoNhanSmartSwap
{
    private sealed class Factory(string connection,string folder):WebApplicationFactory<VongSmartSwapController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder b)=>b.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","src","ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi().ConfigureLogging(l=>l.ClearProviders()).ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>{
                ["ConnectionStrings:ReMarket"]=connection,["Jwt:SigningKey"]=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),["Media:ProductFolder"]=folder,["RateLimits:PermitLimit"]="10000",
                ["Commerce:EnableExpirationWorker"]="false",["Exchange:EnableExpirationWorker"]="false",["SmartSwap:EnableExpirationWorker"]="false"}));
    }
    public static async Task<int> ChayAsync()
    {
        var name="ReMarketTest_"+Guid.NewGuid().ToString("N");var tempRoot=Path.GetFullPath(Path.GetTempPath());var folder=Path.GetFullPath(Path.Combine(tempRoot,"remarket-smart-delivery-"+name));
        var cs=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket")??"Server=localhost;Trusted_Connection=True;TrustServerCertificate=True"){InitialCatalog="master"};
        await using var master=new SqlConnection(cs.ConnectionString);await master.OpenAsync();Factory? factory=null;var created=false;var passed=0;
        void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);passed++;Console.WriteLine("PASS "+text);}
        async Task Status(HttpResponseMessage r,int expected,string text){if((int)r.StatusCode!=expected)throw new InvalidOperationException(text+": "+(int)r.StatusCode+" "+await r.Content.ReadAsStringAsync());Check(true,text+" ["+expected+"]");}
        async Task<T> Post<T>(HttpClient user,string path,object body,int code=200){using var r=await user.PostAsJsonAsync(path,body);await Status(r,code,"POST "+path);return(await r.Content.ReadFromJsonAsync<T>())!;}
        async Task<T> Put<T>(HttpClient user,string path,object body){using var r=await user.PutAsJsonAsync(path,body);await Status(r,200,"PUT "+path);return(await r.Content.ReadFromJsonAsync<T>())!;}
        try
        {
            await new SqlCommand($"CREATE DATABASE [{name}]",master).ExecuteNonQueryAsync();created=true;cs.InitialCatalog=name;factory=new Factory(cs.ConnectionString,folder);
            using var a=factory.CreateClient();using var b=factory.CreateClient();using var c=factory.CreateClient();using var stranger=factory.CreateClient();using var admin=factory.CreateClient();using var anon=factory.CreateClient();
            async Task<T> Db<T>(Func<ReMarketDbContext,Task<T>> fn){await using var scope=factory.Services.CreateAsyncScope();return await fn(scope.ServiceProvider.GetRequiredService<ReMarketDbContext>());}
            async Task Change(Func<ReMarketDbContext,Task> fn)=>await Db(async db=>{await fn(db);await db.SaveChangesAsync();return true;});
            await Db(async db=>{await db.Database.MigrateAsync();Check(!db.Database.HasPendingModelChanges(),"Delivery model matches migration10");return true;});
            async Task<Guid> Account(HttpClient user,string label,string phone)
            {
                var email=label+name+"@example.test";var profile=await Post<HoSoDto>(user,"/api/taikhoan/dangky",new DangKyRequest(label,email,phone,"DeliveryTest!2026","DeliveryTest!2026",true),201);
                var session=await Post<DangNhapDto>(user,"/api/taikhoan/dangnhap",new DangNhapRequest(email,"DeliveryTest!2026"));user.DefaultRequestHeaders.Authorization=new("Bearer",session.AccessToken);return profile.Id;
            }
            var aid=await Account(a,"DeliveryA","0911111111");var bid=await Account(b,"DeliveryB","0922222222");var cid=await Account(c,"DeliveryC","0933333333");var oid=await Account(stranger,"Outsider","0944444444");var adminId=await Account(admin,"Moderator","0955555555");
            var clients=new Dictionary<Guid,HttpClient>{{aid,a},{bid,b},{cid,c}};var actors=new[]{aid,bid,cid};
            var category=new DanhMuc{Ma="delivery-test",Ten="Điện tử giao nhận",DangHienThi=true};
            await Change(async db=>{db.Add(category);db.Add(new NguoiDungVaiTro{NguoiDungId=adminId,VaiTroId=await db.VaiTro.Where(x=>x.Ma=="ADMIN").Select(x=>x.Id).SingleAsync()});});
            byte[] png;using(var bitmap=new SKBitmap(100,80)){bitmap.Erase(SKColors.ForestGreen);using var data=bitmap.Encode(SKEncodedImageFormat.Png,100);png=data.ToArray();}
            async Task<TinDangDto> Publish(HttpClient owner,string title,string wanted,long value)
            {
                var form=new LuuTinDangRequest(title,category.Id,value,"NhuMoi","Sản phẩm kiểm thử, đủ phụ kiện để kiểm tra khi nhận.","Hà Nội","Địa chỉ đăng tin công khai",null,
                    new("Muốn nhận "+wanted,true,true,true,2_000_000,null,new(category.Id,wanted,"DaSuDungTot",0,20_000_000)));
                var tin=await Post<TinDangDto>(owner,"/api/tindang",form,201);
                using var body=new MultipartFormDataContent();body.Add(new StringContent(tin.PhienBan),"phienBan");body.Add(new ByteArrayContent(png),"anh","test.png");
                using var image=await owner.PostAsync($"/api/tindang/{tin.Id}/anh",body);await Status(image,200,"Upload real product image");tin=(await image.Content.ReadFromJsonAsync<TinDangDto>())!;
                tin=await Post<TinDangDto>(owner,$"/api/tindang/{tin.Id}/gui-duyet",new PhienBanTinRequest(tin.PhienBan));return await Post<TinDangDto>(admin,$"/api/admin/tindang/{tin.Id}/duyet",new DuyetTinRequest(tin.PhienBan,true,null));
            }
            async Task<ChiTietVongSmartSwapDto> Circle(string label,bool ready=true)
            {
                var ta=await Publish(a,"iPhone "+label,"Laptop "+label,10_000_000);var tb=await Publish(b,"Laptop "+label,"iPad "+label,12_000_000);var tc=await Publish(c,"iPad "+label,"iPhone "+label,8_000_000);
                var found=(await a.GetFromJsonAsync<KetQuaTimSmartSwapDto>("/api/smart-swaps/search?tinDangId="+ta.Id))!.Items.Single();
                var preview=await Post<XemVongSmartSwapDto>(a,"/api/smart-swaps/preview",new XemVongSmartSwapRequest(found.MaDeXuat));
                var circle=await Post<ChiTietVongSmartSwapDto>(a,"/api/smart-swaps/invitations",new GuiLoiMoiSmartSwapRequest(Guid.NewGuid(),found.MaDeXuat,preview.BamPhuongAn,"Cùng thực hiện giao nhận mô phỏng bằng ba tài khoản.",true),201);
                if(ready)foreach(var user in new[]{b,c})circle=await Post<ChiTietVongSmartSwapDto>(user,$"/api/smart-swaps/{circle.Id}/responses",new PhanHoiSmartSwapRequest(circle.PhuongAnHienTai!.Id,true,true));
                return circle;
            }
            Task<GiaoNhanSmartSwapDto> Get(Guid id,HttpClient? user=null)=>(user??a).GetFromJsonAsync<GiaoNhanSmartSwapDto>($"/api/smart-swaps/{id}/delivery")!;
            async Task<HttpResponseMessage> Evidence(HttpClient user,Guid id,Guid requestKey)
            {using var form=new MultipartFormDataContent();form.Add(new StringContent(requestKey.ToString()),"maYeuCau");form.Add(new ByteArrayContent(png),"anh","evidence.png");return await user.PostAsync($"/api/smart-swaps/{id}/evidence",form);}
            XacNhanLienHeSmartSwapRequest Contact(Guid actor,Guid version)=>new(Guid.NewGuid(),version,"Liên hệ "+actor.ToString("N")[..5],"+84901234567","123 Đường Gửi, Hà Nội","456 Đường Nhận, Hà Nội",true);
            async Task<GiaoNhanSmartSwapDto> Start(ChiTietVongSmartSwapDto circle)
            {foreach(var actor in actors)await Put<GiaoNhanSmartSwapDto>(clients[actor],$"/api/smart-swaps/{circle.Id}/delivery/contact",Contact(actor,circle.PhuongAnHienTai!.Id));return await Post<GiaoNhanSmartSwapDto>(a,$"/api/smart-swaps/{circle.Id}/shipments",new TaoVanChuyenSmartSwapRequest(circle.PhuongAnHienTai!.Id));}
            async Task<GiaoNhanSmartSwapDto> Delivered(ChiTietVongSmartSwapDto circle,GiaoNhanSmartSwapDto delivery)
            {
                foreach(var original in delivery.VanChuyen)
                {
                    var ship=original;var user=clients[ship.NguoiGuiId];
                    var after=await Put<GiaoNhanSmartSwapDto>(user,$"/api/smart-swaps/{circle.Id}/shipments/{ship.Id}/tracking",new MaVanDonSmartSwapRequest(ship.PhienBan,"TEST-"+ship.Id.ToString("N")[..12]));ship=after.VanChuyen.Single(x=>x.Id==ship.Id);
                    foreach(var next in new[]{"DaGui","DangVanChuyen","DaGiao"}){after=await Post<GiaoNhanSmartSwapDto>(user,$"/api/smart-swaps/{circle.Id}/shipments/{ship.Id}/status",new ChuyenVanChuyenSmartSwapRequest(ship.PhienBan,next,true));ship=after.VanChuyen.Single(x=>x.Id==ship.Id);}
                }
                return(await Get(circle.Id))!;
            }
            async Task<GiaoNhanSmartSwapDto> Receive(ChiTietVongSmartSwapDto circle,Guid actor)
            {var d=(await Get(circle.Id,clients[actor]))!;var ship=d.VanChuyen.Single(x=>x.NguoiNhanId==actor);return await Post<GiaoNhanSmartSwapDto>(clients[actor],$"/api/smart-swaps/{circle.Id}/shipments/{ship.Id}/received",new NhanHangSmartSwapRequest(circle.PhuongAnHienTai!.Id,true,true,true));}
            async Task SqlReject(FormattableString sql,string text)
            {await Db(async db=>{await using var tx=await db.Database.BeginTransactionAsync();var rejected=false;try{await db.Database.ExecuteSqlInterpolatedAsync(sql);}catch(SqlException ex)when(ex.Number is 547 or 51001 or 51006 or 51030 or 51031 or 51032 or 51033 or 51034){rejected=true;}finally{await tx.RollbackAsync();}Check(rejected,text);return true;});}

            var pending=await Circle("Pending",false);
            DanhGiaSmartSwapRequest Reviews(Guid actor)=>new(Guid.NewGuid(),actors.Where(x=>x!=actor).Select(x=>new DanhGiaThanhVienSmartSwapRequest(x,5,"Thành viên đúng hẹn, sản phẩm phù hợp mô tả.")).ToList());
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{pending.Id}/reviews",Reviews(aid)),409,"Pending circle cannot be reviewed");
            await Status(await Evidence(a,pending.Id,Guid.NewGuid()),409,"Evidence cannot substitute an explicit correct receipt");
            var ownPending=pending.ThanhVien.Single(x=>x.NguoiDungId==aid).SanPhamCho.TinDangId;
            var narrow=await a.GetFromJsonAsync<KetQuaTimSmartSwapDto>($"/api/smart-swaps/search?tinDangId={ownPending}&tuKhoaMuonNhan=Laptop&buToiDa=2000000&tinhTrangToiThieu=NhuMoi");
            Check(narrow!.TongSo==1&&narrow.HoSo?.Count==3,"Search form filters preserve matching cycle and return three actual member names");
            Check((await a.GetFromJsonAsync<KetQuaTimSmartSwapDto>($"/api/smart-swaps/search?tinDangId={ownPending}&buToiDa=0"))!.TongSo==0,"Search maximum cash filters before pagination");
            Check((await a.GetFromJsonAsync<KetQuaTimSmartSwapDto>($"/api/smart-swaps/search?tinDangId={ownPending}&tuKhoaMuonNhan=Camera"))!.TongSo==0,"Unmatched receiving-product search is truly empty");
            await Status(await a.GetAsync("/api/smart-swaps/search?tuKhoaMuonNhan=Laptop"),400,"Receiving filters require an anchor listing");
            await Status(await a.GetAsync($"/api/smart-swaps/search?tinDangId={ownPending}&chiThanhVienDaXacMinh=true"),404,"Verified filter never labels unverified accounts as verified");
            await Status(await a.PutAsJsonAsync($"/api/smart-swaps/{pending.Id}/delivery/contact",Contact(aid,pending.PhuongAnHienTai!.Id)),409,"Contact cannot precede three consents");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{pending.Id}/shipments",new TaoVanChuyenSmartSwapRequest(pending.PhuongAnHienTai.Id)),409,"Cannot create shipments before official circle");
            await Post<ChiTietVongSmartSwapDto>(a,$"/api/smart-swaps/{pending.Id}/cancel",new HuyVongSmartSwapRequest(pending.PhuongAnHienTai.Id,"Hủy vòng chưa đủ đồng thuận của kiểm thử."));
            var v=await Circle("Complete");var version=v.PhuongAnHienTai!.Id;
            await Status(await anon.GetAsync($"/api/smart-swaps/{v.Id}/delivery"),401,"Anonymous delivery denied");await Status(await stranger.GetAsync($"/api/smart-swaps/{v.Id}/delivery"),404,"Outsider delivery privacy");
            var contactBody=Contact(aid,version);
            await Status(await a.PutAsJsonAsync($"/api/smart-swaps/{v.Id}/delivery/contact",contactBody with{SoDienThoai="123"}),400,"Contact phone validation");
            await Status(await a.PutAsJsonAsync($"/api/smart-swaps/{v.Id}/delivery/contact",contactBody with{XacNhanThongTin=false}),400,"Explicit personal contact consent required");
            var d=await Put<GiaoNhanSmartSwapDto>(a,$"/api/smart-swaps/{v.Id}/delivery/contact",contactBody);
            Check(d.SoNguoiXacNhanThongTin==1&&d.LienHe.Single(x=>x.NguoiDungId==aid).SoDienThoai=="0901234567","Own contact normalized and saved");
            await Put<GiaoNhanSmartSwapDto>(a,$"/api/smart-swaps/{v.Id}/delivery/contact",contactBody);
            Check(await Db(db=>db.ThongTinGiaoNhanSmartSwap.CountAsync())==1,"Contact retry does not add another record");
            await Status(await a.PutAsJsonAsync($"/api/smart-swaps/{v.Id}/delivery/contact",contactBody with{DiaChiNhan="999 Địa chỉ khác, Hà Nội"}),409,"Contact key payload protection");
            await Put<GiaoNhanSmartSwapDto>(b,$"/api/smart-swaps/{v.Id}/delivery/contact",Contact(bid,version));
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments",new TaoVanChuyenSmartSwapRequest(version)),409,"Two contacts cannot create a partial shipping set");
            Check(await Db(db=>db.VanChuyenMoPhong.CountAsync())==0,"Failed start leaves zero shipments");
            await Put<GiaoNhanSmartSwapDto>(c,$"/api/smart-swaps/{v.Id}/delivery/contact",Contact(cid,version));
            d=await Post<GiaoNhanSmartSwapDto>(a,$"/api/smart-swaps/{v.Id}/shipments",new TaoVanChuyenSmartSwapRequest(version));
            Check(d.VanChuyen.Count==3&&d.VanChuyen.All(x=>x.TrangThai=="ChoGui")&&d.TienDo.TienDo=="0/3","Exactly three waiting simulated shipments linked to one circle");
            await Post<GiaoNhanSmartSwapDto>(b,$"/api/smart-swaps/{v.Id}/shipments",new TaoVanChuyenSmartSwapRequest(version));Check(await Db(db=>db.VanChuyenMoPhong.CountAsync())==3,"Shipment creation retry is idempotent");
            var view=(await Get(v.Id))!;Check(view.LienHe.Where(x=>x.NguoiDungId!=aid).All(x=>x.SoDienThoai==null&&x.DiaChiNhan==null),"Other members' unneeded contact fields redacted");
            Check(view.VanChuyen.Single(x=>x.NguoiGuiId!=aid&&x.NguoiNhanId!=aid).DiaChiNhan==null,"Unrelated leg hides private shipping address");
            await Status(await a.PutAsJsonAsync($"/api/smart-swaps/{v.Id}/delivery/contact",contactBody with{MaYeuCau=Guid.NewGuid(),PhienBan=d.LienHe.Single(x=>x.NguoiDungId==aid).PhienBan}),409,"Address snapshot frozen after booking");
            var own=view.VanChuyen.Single(x=>x.NguoiGuiId==aid);var otherLeg=view.VanChuyen.Single(x=>x.NguoiGuiId==bid);
            await Status(await b.PutAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{own.Id}/tracking",new MaVanDonSmartSwapRequest(own.PhienBan,"NOT-MY-LEG")),403,"Only designated sender edits tracking");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{own.Id}/status",new ChuyenVanChuyenSmartSwapRequest(own.PhienBan,"DaGiao",true)),409,"Cannot skip shipping states");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{own.Id}/status",new ChuyenVanChuyenSmartSwapRequest(own.PhienBan,"DaGui",true)),400,"Tracking code required to send");
            var updated=await Put<GiaoNhanSmartSwapDto>(a,$"/api/smart-swaps/{v.Id}/shipments/{own.Id}/tracking",new MaVanDonSmartSwapRequest(own.PhienBan,"MANUAL-TEST-ONE"));own=updated.VanChuyen.Single(x=>x.Id==own.Id);
            await Status(await b.PutAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{otherLeg.Id}/tracking",new MaVanDonSmartSwapRequest(otherLeg.PhienBan,"manual-test-one")),409,"Duplicate tracking rejected case-insensitively");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{own.Id}/status",new ChuyenVanChuyenSmartSwapRequest(own.PhienBan,"DaGui",false)),400,"Sender must explicitly confirm shipment");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{own.Id}/received",new NhanHangSmartSwapRequest(version,true,true,true)),403,"Sender cannot confirm receipt for assigned recipient");
            await Status(await clients[own.NguoiNhanId].PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{own.Id}/received",new NhanHangSmartSwapRequest(version,true,true,true)),409,"Recipient cannot confirm before delivered state");
            // Refresh and drive all legs, including the one with manually entered tracking.
            d=await Delivered(v,(await Get(v.Id))!);
            Check(d.VanChuyen.All(x=>x.TrangThai=="DaGiao")&&d.TienDo.SoNguoiNhanDung==0&&!d.TienDo.HoanTat,"Carrier delivered labels do NOT auto-confirm any receiver");
            Check(await Db(db=>db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.Loai==LoaiXacNhan.DaGiao))==3&&await Db(db=>db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.Loai==LoaiXacNhan.DaNhan))==0,"Only three explicit sender dispatch receipts saved so far");
            var incoming=d.VanChuyen.Single(x=>x.NguoiNhanId==aid);
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{incoming.Id}/received",new NhanHangSmartSwapRequest(version,true,false,true)),400,"Wrong goods are not treated as successful receipt");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/shipments/{incoming.Id}/received",new NhanHangSmartSwapRequest(version,true,true,false)),400,"No automatic confirmation of actor's compensation");
            d=await Receive(v,aid);Check(d.TienDo.TienDo=="1/3"&&d.TrangThaiGiaoDich=="DangThucHien","First correct receiver advances only to1/3");
            await Receive(v,aid);Check(await Db(db=>db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.Loai==LoaiXacNhan.DaNhan))==1,"Repeated receipt is idempotent");
            d=await Receive(v,bid);Check(d.TienDo.TienDo=="2/3"&&!d.TienDo.HoanTat&&d.TrangThaiGiaoDich=="DangThucHien","Two correct receivers DO NOT complete circle");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/reviews",Reviews(aid)),409,"Two of three receipts cannot unlock review");
            Check(await Db(db=>db.GiuChoSanPham.CountAsync(x=>x.GiaoDichId==v.GiaoDichId&&!x.DaGiaiPhong))==3,"All products stay reserved at2/3");
            await SqlReject($"UPDATE VongSmartSwap SET TrangThai='HoanTat' WHERE Id={v.Id}","SQL rejects completion at2/3");
            d=await Receive(v,cid);Check(d.TienDo.TienDo=="3/3"&&d.TienDo.HoanTat&&d.TrangThaiVong=="HoanTat"&&d.TrangThaiGiaoDich=="HoanTat","Only third designated correct receipt completes both circle and root");
            Check(await Db(db=>db.GiuChoSanPham.CountAsync(x=>x.GiaoDichId==v.GiaoDichId&&!x.DaGiaiPhong))==0,"Completion releases all three holds");
            Check(await Db(db=>db.TinDang.Where(x=>v.ThanhVien.Select(m=>m.SanPhamCho.TinDangId).Contains(x.Id)).AllAsync(x=>x.TrangThai==TrangThaiTinDang.DaTraoDoi)),"Completed products marked exchanged");
            await Receive(v,cid);Check(await Db(db=>db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==v.GiaoDichId))==6,"Completed retry never duplicates six explicit sender/receiver records");
            var chat=await Post<HoiThoaiDto>(b,$"/api/hoithoai/{v.HoiThoaiId}/tinnhan",new GuiTinRequest(Guid.NewGuid(),"Cảm ơn nhóm, tôi đã nhận đúng sản phẩm."));Check(chat.TinNhan.Any(x=>x.NoiDung!.Contains("Cảm ơn nhóm")),"Group chat persists real messages after delivery");
            Check((await c.GetFromJsonAsync<List<ThongBaoSmartSwapDto>>("/api/smart-swaps/notifications"))!.Any(x=>x.VongId==v.Id&&x.TieuDe.Contains("hoàn tất")),"Final state notification available to third account");
            await Status(await anon.GetAsync($"/api/smart-swaps/{v.Id}/reviews"),401,"Anonymous reviews denied");
            await Status(await stranger.GetAsync($"/api/smart-swaps/{v.Id}/reviews"),404,"Outsider reviews remain private");
            await Status(await stranger.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/reviews",Reviews(aid)),404,"Outsider cannot submit circle reviews");
            var invalid=Reviews(aid) with{DanhGia=[new(aid,5,"Không được tự đánh giá chính mình."),new(bid,5,"Thành viên đúng hẹn và thân thiện.")]};
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/reviews",invalid),400,"Cannot rate self or wrong pair");
            Check(await Db(db=>db.DanhGia.CountAsync(x=>x.GiaoDichId==v.GiaoDichId))==0,"Invalid pair leaves no partial review");
            foreach(var actor in actors)
            {
                var request=Reviews(actor);var rows=await Post<List<DanhGiaSmartSwapDto>>(clients[actor],$"/api/smart-swaps/{v.Id}/reviews",request);
                Check(rows.Count==2&&rows.All(x=>x.NguoiDuocDanhGiaId!=actor&&x.SoSao==5),"Each separate actor can rate both other members");
                await Post<List<DanhGiaSmartSwapDto>>(clients[actor],$"/api/smart-swaps/{v.Id}/reviews",request);
                await Status(await clients[actor].PostAsJsonAsync($"/api/smart-swaps/{v.Id}/reviews",request with{MaYeuCau=Guid.NewGuid()}),409,"A different request cannot overwrite existing reviews");
            }
            Check(await Db(db=>db.DanhGia.CountAsync(x=>x.GiaoDichId==v.GiaoDichId))==6,"Three accounts persist exactly six reviews without retry duplicates");
            Check((await a.GetFromJsonAsync<List<DanhGiaSmartSwapDto>>($"/api/smart-swaps/{v.Id}/reviews"))!.Count==2,"Authenticated participant review-list endpoint returns both actual submitted reviews");
            var evidenceKey=Guid.NewGuid();var photoResponse=await Evidence(a,v.Id,evidenceKey);await Status(photoResponse,200,"Upload real Smart Swap receipt evidence");
            var evidence=(await photoResponse.Content.ReadFromJsonAsync<GiaoNhanSmartSwapDto>())!.BangChung!.Single();
            await Status(await Evidence(a,v.Id,evidenceKey),200,"Evidence retry uses the same record");
            Check(await Db(db=>db.BangChungGiaoNhan.CountAsync(x=>x.XacNhanGiaoNhan.GiaoDichId==v.GiaoDichId))==1,"Evidence retry does not duplicate files in SQL");
            await Status(await b.GetAsync("/"+evidence.Url),200,"Other member can read real receipt evidence");
            await Status(await stranger.GetAsync("/"+evidence.Url),404,"Evidence is private to circle members");

            foreach(var kind in new[]{"SaiHang","ChuaNhanHang"})
            {
                var bad=await Circle(kind);var state=await Start(bad);if(kind=="SaiHang")state=await Delivered(bad,state);
                var received=state.VanChuyen.Single(x=>x.NguoiNhanId==bid);var issue=new BaoSuCoSmartSwapRequest(Guid.NewGuid(),received.Id,kind,"Vấn đề giao nhận thử nghiệm","Tôi nhận sai sản phẩm hoặc chưa nhận được hàng theo thỏa thuận.");
                await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{bad.Id}/issues",issue),403,"Only assigned recipient reports its receiving issue");
                state=await Post<GiaoNhanSmartSwapDto>(b,$"/api/smart-swaps/{bad.Id}/issues",issue);
                Check(state.TienDo.TamDung&&state.TrangThaiVong=="TamDung"&&state.TrangThaiGiaoDich=="TranhChap"&&state.SuCo.Single().LoaiSuCo==kind,"Incident persists and atomically pauses entire circle: "+kind);
                await Status(await b.PostAsJsonAsync($"/api/smart-swaps/{bad.Id}/reviews",Reviews(bid)),409,"Incident circle cannot be reviewed");
                await Post<GiaoNhanSmartSwapDto>(b,$"/api/smart-swaps/{bad.Id}/issues",issue);Check(await Db(db=>db.KhieuNai.CountAsync(x=>x.GiaoDichId==bad.GiaoDichId))==1,"Incident retry does not duplicate complaint");
                await Status(await b.PostAsJsonAsync($"/api/smart-swaps/{bad.Id}/issues",issue with{NoiDung="Nội dung khác dùng cùng mã báo cáo sự cố."}),409,"Incident key payload protected");
                await Status(await b.PostAsJsonAsync($"/api/smart-swaps/{bad.Id}/shipments/{received.Id}/received",new NhanHangSmartSwapRequest(bad.PhuongAnHienTai!.Id,true,true,true)),409,"Paused circle blocks correct-receipt mutation");
                var outgoing=state.VanChuyen.Single(x=>x.NguoiGuiId==aid);
                await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{bad.Id}/shipments/{outgoing.Id}/status",new ChuyenVanChuyenSmartSwapRequest(outgoing.PhienBan,"DaGui",true)),409,"Paused circle blocks shipping mutation");
                Check(await Db(db=>db.GiuChoSanPham.CountAsync(x=>x.GiaoDichId==bad.GiaoDichId&&!x.DaGiaiPhong))==3,"Incident preserves reservations for dispute handling");
                await SqlReject($"UPDATE VongSmartSwap SET TrangThai='HoanTat' WHERE Id={bad.Id}","SQL cannot complete incident circle");
                await Post<HoiThoaiDto>(c,$"/api/hoithoai/{bad.HoiThoaiId}/tinnhan",new GuiTinRequest(Guid.NewGuid(),"Nhóm tiếp tục trao đổi để làm rõ sự cố, không tự xác nhận."));
            }
            var race=await Circle("Race");var raceDelivery=await Delivered(race,await Start(race));await Receive(race,aid);await Receive(race,bid);var last=raceDelivery.VanChuyen.Single(x=>x.NguoiNhanId==cid);
            var parallel=await Task.WhenAll(c.PostAsJsonAsync($"/api/smart-swaps/{race.Id}/shipments/{last.Id}/received",new NhanHangSmartSwapRequest(race.PhuongAnHienTai!.Id,true,true,true)),
                c.PostAsJsonAsync($"/api/smart-swaps/{race.Id}/issues",new BaoSuCoSmartSwapRequest(Guid.NewGuid(),last.Id,"ChuaNhanHang","Báo cáo cạnh tranh với xác nhận","Kiểm tra giao dịch khi nhận đúng và báo cáo được gửi đồng thời.")));
            Check(parallel.Count(x=>(int)x.StatusCode==200)==1&&parallel.Count(x=>(int)x.StatusCode==409)==1,"Final receipt vs incident serialized: exactly one accepted, one409");
            var final=(await Get(race.Id))!;Check(final.TienDo.HoanTat&&!final.TienDo.TamDung||final.TienDo.TamDung&&!final.TienDo.HoanTat,"Circle cannot be simultaneously paused and completed");

            await Db(async db=>{await DuLieuMau.KhoiTaoAsync(db);return true;});Guid demo;
            await using(var scope=factory.Services.CreateAsyncScope())demo=await scope.ServiceProvider.GetRequiredService<DemoGiaoNhanSmartSwap>().KhoiTaoAsync();
            var countBefore=await Db(db=>db.VongSmartSwap.CountAsync());
            await using(var scope=factory.Services.CreateAsyncScope())Check(demo==await scope.ServiceProvider.GetRequiredService<DemoGiaoNhanSmartSwap>().KhoiTaoAsync(),"Delivery demo seed returns same circle id");
            Check(countBefore==await Db(db=>db.VongSmartSwap.CountAsync()),"Delivery demo rerun creates no duplicate circle");
            using var demoC=factory.CreateClient();var secret=Environment.GetEnvironmentVariable("REMARKET_DEMO_PASSWORD")??"ReMarketDemo!2026";
            var session=await Post<DangNhapDto>(demoC,"/api/taikhoan/dangnhap",new DangNhapRequest(DemoGiaoNhanSmartSwap.Emails[2],secret));demoC.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",session.AccessToken);
            var sample=(await Get(demo,demoC))!;Check(sample.TienDo.TienDo=="2/3"&&!sample.TienDo.HoanTat,"Persistent demo starts at2/3 with final actor still pending");
            var profile=(await demoC.GetFromJsonAsync<HoSoDto>("/api/taikhoan/toi"))!;var incomingDemo=sample.VanChuyen.Single(x=>x.NguoiNhanId==profile.Id);
            sample=await Post<GiaoNhanSmartSwapDto>(demoC,$"/api/smart-swaps/{demo}/shipments/{incomingDemo.Id}/received",new NhanHangSmartSwapRequest(sample.PhienBanThoaThuanId!.Value,true,true,true));Check(sample.TienDo.HoanTat,"Demo third separate account can finish real SQL circle");
            await using(var scope=factory.Services.CreateAsyncScope())await scope.ServiceProvider.GetRequiredService<DemoGiaoNhanSmartSwap>().KhoiTaoAsync();
            Check((await Get(demo,demoC))!.TienDo.HoanTat,"Reseeding does not reset a completed demo");
            Console.WriteLine($"Smart Swap delivery HTTP/SQL checks: {passed} passed. Three separate JWT accounts; explicit0/3,1/3,2/3,3/3 receipts and incident pause.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine("FAIL Smart Swap delivery: "+ex);return 1;}
        finally
        {
            if(factory!=null)await factory.DisposeAsync();if(created&&name.StartsWith("ReMarketTest_",StringComparison.Ordinal))await new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]",master).ExecuteNonQueryAsync();
            if(Directory.Exists(folder)&&folder.StartsWith(tempRoot,StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(folder)=="remarket-smart-delivery-"+name)Directory.Delete(folder,true);
        }
    }
}
