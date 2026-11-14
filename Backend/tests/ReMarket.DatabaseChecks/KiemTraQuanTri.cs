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
using ReMarket.Application.QuanTri;
using ReMarket.Application.SanPham;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;
using ReMarket.Infrastructure.QuanTri;
using ReMarket.Infrastructure.Seeding;
using SkiaSharp;

namespace ReMarket.DatabaseChecks;
internal static class KiemTraQuanTri
{
    private sealed class Factory(string cs,string folder):WebApplicationFactory<QuanTriController>{protected override void ConfigureWebHost(IWebHostBuilder b)=>b.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","src","ReMarket.Api"))).UseEnvironment("Development").ObserveApi().ConfigureLogging(l=>{l.ClearProviders();l.AddSimpleConsole();l.SetMinimumLevel(LogLevel.Error);}).ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>{
        ["ConnectionStrings:ReMarket"]=cs,["Jwt:SigningKey"]=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),["Media:ProductFolder"]=folder,["RateLimits:PermitLimit"]="10000",["Admin:EnableUnlockWorker"]="false",["Commerce:EnableExpirationWorker"]="false",["Exchange:EnableExpirationWorker"]="false",["SmartSwap:EnableExpirationWorker"]="false"}));}
    public static async Task<int> ChayAsync()
    {
        var name="ReMarketTest_"+Guid.NewGuid().ToString("N");var root=Path.GetFullPath(Path.GetTempPath());var folder=Path.GetFullPath(Path.Combine(root,"remarket-admin-"+name));
        var cs=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket")??"Server=localhost;Trusted_Connection=True;TrustServerCertificate=True"){InitialCatalog="master"};
        await using var master=new SqlConnection(cs.ConnectionString);await master.OpenAsync();Factory? f=null;var created=false;var count=0;
        void Check(bool value,string text){if(!value)throw new InvalidOperationException(text);count++;Console.WriteLine("PASS "+text);}
        async Task Status(HttpResponseMessage response,int code,string text){if((int)response.StatusCode!=code)throw new InvalidOperationException(text+": "+(int)response.StatusCode+" "+await response.Content.ReadAsStringAsync());Check(true,text+" ["+code+"]");}
        async Task<T> Post<T>(HttpClient c,string path,object body,int code=200){var r=await c.PostAsJsonAsync(path,body);await Status(r,code,"POST "+path);return(await r.Content.ReadFromJsonAsync<T>())!;}
        async Task<T> Put<T>(HttpClient c,string path,object body){var r=await c.PutAsJsonAsync(path,body);await Status(r,200,"PUT "+path);return(await r.Content.ReadFromJsonAsync<T>())!;}
        try{
            await new SqlCommand($"CREATE DATABASE [{name}]",master).ExecuteNonQueryAsync();created=true;cs.InitialCatalog=name;f=new Factory(cs.ConnectionString,folder);
            using var admin=f.CreateClient();using var user=f.CreateClient();using var anon=f.CreateClient();using var second=f.CreateClient();
            async Task<T> Db<T>(Func<ReMarketDbContext,Task<T>> fn){await using var scope=f.Services.CreateAsyncScope();return await fn(scope.ServiceProvider.GetRequiredService<ReMarketDbContext>());}
            async Task Change(Func<ReMarketDbContext,Task> fn)=>await Db(async db=>{await fn(db);await db.SaveChangesAsync();return true;});
            await Db(async db=>{await db.Database.MigrateAsync();Check(!db.Database.HasPendingModelChanges(),"Admin model matches migration11");await DuLieuMau.KhoiTaoAsync(db);return true;});
            const string password="AdminChecks!2026";
            var accountNumber=10;
            async Task<Guid> Account(HttpClient client,string label){var email=label+name+"@example.test";var profile=await Post<HoSoDto>(client,"/api/taikhoan/dangky",new DangKyRequest(label,email,"07"+(++accountNumber).ToString("D8"),password,password,true),201);var login=await Post<DangNhapDto>(client,"/api/taikhoan/dangnhap",new DangNhapRequest(email,password));client.DefaultRequestHeaders.Authorization=new("Bearer",login.AccessToken);return profile.Id;}
            var aid=await Account(admin,"Admin");var uid=await Account(user,"Subject");var sid=await Account(second,"Other");
            await Change(async db=>db.Add(new NguoiDungVaiTro{NguoiDungId=aid,VaiTroId=await db.VaiTro.Where(x=>x.Ma=="ADMIN").Select(x=>x.Id).SingleAsync()}));
            var random=Guid.NewGuid();var body=new{};
            var routes=new[]{("GET","bao-cao"),("GET","nguoi-dung"),("GET",$"nguoi-dung/{uid}"),("POST",$"nguoi-dung/{uid}/khoa"),("POST",$"nguoi-dung/{uid}/mo-khoa"),("GET","danh-muc"),("POST","danh-muc"),("PUT",$"danh-muc/{random}"),("GET","san-pham"),("GET",$"san-pham/{random}"),("POST",$"san-pham/{random}/duyet"),("POST",$"san-pham/{random}/an"),("GET","giao-dich"),("GET",$"giao-dich/{random}"),("GET",$"giao-dich/{random}/media/product/{random}"),("GET","khieu-nai"),("GET",$"khieu-nai/{random}"),("POST",$"khieu-nai/{random}/tiep-nhan"),("POST",$"khieu-nai/{random}/quyet-dinh"),("POST",$"khieu-nai/{random}/xac-minh")};
            foreach(var(method,path) in routes.Append(("POST",$"giao-dich/{random}/khieu-nai"))){foreach(var(client,code) in new[]{(anon,401),(user,403)}){using var request=new HttpRequestMessage(new HttpMethod(method),"/api/admin/"+path);if(method!="GET")request.Content=JsonContent.Create(body);await Status(await client.SendAsync(request),code,"Admin boundary "+method+" "+path);}}
            await using(var checkScope=f.Services.CreateAsyncScope())await checkScope.ServiceProvider.GetRequiredService<QuanTriService>().BaoCaoAsync(aid,null,null,default);
            var report=(await admin.GetFromJsonAsync<BaoCaoQuanTriDto>("/api/admin/bao-cao"))!;Check(report.TongNguoiDung==await Db(db=>db.NguoiDung.CountAsync()),"SQL user count, not a hardcoded KPI");
            Check((await admin.GetFromJsonAsync<TrangQuanTri<NguoiDungQuanTriDto>>("/api/admin/nguoi-dung?q=Subject"))!.Items.Any(x=>x.Id==uid),"Admin user search returns real user");
            Check((await admin.GetFromJsonAsync<List<DanhMucQuanTriDto>>("/api/admin/danh-muc"))!.Count>0,"Admin reads category tree");
            await Status(await admin.GetAsync("/api/admin/san-pham"),200,"Admin listing queue accessible");
            Check((await admin.GetFromJsonAsync<TrangQuanTri<GiaoDichQuanTriDto>>("/api/admin/giao-dich"))!.Items.Count>0,"Admin transaction list contains SQL seed transactions");
            Check((await admin.GetFromJsonAsync<TrangQuanTri<KhieuNaiQuanTriDto>>("/api/admin/khieu-nai"))!.Items.Count>0,"Admin complaint list contains SQL cases");
            await Status(await admin.GetAsync("/api/admin/bao-cao?tuNgay=2026-11-12&denNgay=2026-01-01"),400,"Date interval validation");
            await Status(await admin.GetAsync("/api/admin/bao-cao?tuNgay=9999-12-31&denNgay=9999-12-31"),400,"Report maximum date does not overflow into500");
            await Status(await admin.GetAsync("/api/admin/bao-cao?tuNgay=0001-01-01&denNgay=0001-01-01"),400,"Report previous-period date does not underflow");
            var detail=(await admin.GetFromJsonAsync<ChiTietNguoiDungQuanTriDto>($"/api/admin/nguoi-dung/{uid}"))!;
            await Status(await admin.PostAsJsonAsync($"/api/admin/nguoi-dung/{uid}/khoa",new KhoaNguoiDungRequest(detail.NguoiDung.PhienBan,"Lý do kiểm thử",1)),400,"Only supported lock durations");
            var own=(await admin.GetFromJsonAsync<ChiTietNguoiDungQuanTriDto>($"/api/admin/nguoi-dung/{aid}"))!;await Status(await admin.PostAsJsonAsync($"/api/admin/nguoi-dung/{aid}/khoa",new KhoaNguoiDungRequest(own.NguoiDung.PhienBan,"Không được tự khóa",7)),403,"Admin cannot lock self");
            detail=await Post<ChiTietNguoiDungQuanTriDto>(admin,$"/api/admin/nguoi-dung/{uid}/khoa",new KhoaNguoiDungRequest(detail.NguoiDung.PhienBan,"Đăng tin trùng lặp cần kiểm tra",7));Check(detail.NguoiDung.TrangThai=="DaKhoa"&&detail.Khoa.First().NgayKetThuc!=null,"Lock record, deadline and status persisted");
            await Status(await user.GetAsync("/api/taikhoan/toi"),401,"Lock immediately revokes current JWT/session");
            await Status(await anon.PostAsJsonAsync("/api/taikhoan/dangnhap",new DangNhapRequest("Subject"+name+"@example.test",password)),401,"Locked account cannot login");
            await Status(await admin.PostAsJsonAsync($"/api/admin/nguoi-dung/{uid}/mo-khoa",new MoKhoaNguoiDungRequest(detail.NguoiDung.PhienBan,"Đã kiểm tra",false)),400,"Unlock requires explicit review");
            detail=await Post<ChiTietNguoiDungQuanTriDto>(admin,$"/api/admin/nguoi-dung/{uid}/mo-khoa",new MoKhoaNguoiDungRequest(detail.NguoiDung.PhienBan,"Đã rà soát, đủ điều kiện",true));Check(detail.NguoiDung.TrangThai=="HoatDong"&&detail.Khoa.All(x=>x.NgayMoKhoa!=null),"Unlock preserves history");await Status(await user.GetAsync("/api/taikhoan/toi"),401,"Unlock never restores revoked tokens");
            var session=await Post<DangNhapDto>(anon,"/api/taikhoan/dangnhap",new DangNhapRequest("Subject"+name+"@example.test",password));user.DefaultRequestHeaders.Authorization=new("Bearer",session.AccessToken);
            detail=await Post<ChiTietNguoiDungQuanTriDto>(admin,$"/api/admin/nguoi-dung/{uid}/khoa",new KhoaNguoiDungRequest(detail.NguoiDung.PhienBan,"Thử hết hạn tự động",7));
            await Change(async db=>{var l=await db.KhoaTaiKhoan.SingleAsync(x=>x.NguoiDungId==uid&&x.NgayMoKhoa==null);l.NgayBatDau=DateTimeOffset.UtcNow.AddDays(-8);l.NgayKetThuc=DateTimeOffset.UtcNow.AddDays(-1);});
            await using(var scope=f.Services.CreateAsyncScope())Check(await scope.ServiceProvider.GetRequiredService<QuanTriService>().MoKhoaHetHanAsync(default)>=1,"Expired timed lock is released by worker service");
            var parent=await Post<DanhMucQuanTriDto>(admin,"/api/admin/danh-muc",new LuuDanhMucQuanTriRequest("Danh mục kiểm thử",null,1,true,"smartphone"),201);
            var child=await Post<DanhMucQuanTriDto>(admin,"/api/admin/danh-muc",new LuuDanhMucQuanTriRequest("Danh mục con",parent.Id,2,true,"book-open"),201);
            await Status(await admin.PutAsJsonAsync($"/api/admin/danh-muc/{parent.Id}",new LuuDanhMucQuanTriRequest(parent.Ten,child.Id,1,true,"smartphone",parent.PhienBan)),400,"Category cycle rejected");
            var updated=await Put<DanhMucQuanTriDto>(admin,$"/api/admin/danh-muc/{child.Id}",new LuuDanhMucQuanTriRequest("Sách cập nhật",parent.Id,3,false,"book-open",child.PhienBan));Check(!updated.DangHienThi&&updated.BieuTuong=="book-open","Category icon/order/visibility saved");
            var publicCats=(await anon.GetFromJsonAsync<List<DanhMucDto>>("/api/tindang/danhmuc"))!;Check(publicCats.Any(x=>x.Id==parent.Id&&x.BieuTuong=="smartphone")&&publicCats.All(x=>x.Id!=child.Id),"Public category uses saved icon and hides disabled category");
            await Status(await admin.PutAsJsonAsync($"/api/admin/danh-muc/{child.Id}",new LuuDanhMucQuanTriRequest("Tên cũ",null,0,true,"shirt",child.PhienBan)),409,"Stale category edit rejected");
            session=await Post<DangNhapDto>(anon,"/api/taikhoan/dangnhap",new DangNhapRequest("Subject"+name+"@example.test",password));user.DefaultRequestHeaders.Authorization=new("Bearer",session.AccessToken);
            async Task<TinDangDto> Pending(){var t=await Post<TinDangDto>(user,"/api/tindang",new LuuTinDangRequest("Sản phẩm kiểm duyệt",parent.Id,500000,"NhuMoi","Mô tả đầy đủ sản phẩm kiểm thử quản trị.","Hà Nội","Hà Nội",null),201);byte[] png;using(var bitmap=new SKBitmap(100,80)){bitmap.Erase(SKColors.Green);png=bitmap.Encode(SKEncodedImageFormat.Png,100).ToArray();}using var form=new MultipartFormDataContent();form.Add(new StringContent(t.PhienBan),"phienBan");form.Add(new ByteArrayContent(png),"anh","qa.png");var response=await user.PostAsync($"/api/tindang/{t.Id}/anh",form);await Status(response,200,"Upload moderation photo");t=(await response.Content.ReadFromJsonAsync<TinDangDto>())!;return await Post<TinDangDto>(user,$"/api/tindang/{t.Id}/gui-duyet",new PhienBanTinRequest(t.PhienBan));}
            var listing=await Pending();await Status(await admin.GetAsync($"/api/admin/san-pham/{listing.Id}"),200,"Admin reads actual pending listing");var mod=await Post<ChiTietTinQuanTriDto>(admin,$"/api/admin/san-pham/{listing.Id}/duyet",new DuyetTinRequest(listing.PhienBan,false,"Cần bổ sung ảnh chi tiết"));Check(mod.Tin.TrangThai=="TuChoi"&&mod.LichSu.Count==1,"Reject reason and moderation history");
            listing=await Pending();mod=await Post<ChiTietTinQuanTriDto>(admin,$"/api/admin/san-pham/{listing.Id}/duyet",new DuyetTinRequest(listing.PhienBan,true,null));mod=await Post<ChiTietTinQuanTriDto>(admin,$"/api/admin/san-pham/{listing.Id}/an",new AnTinQuanTriRequest(mod.Tin.PhienBan,"Ẩn tin sau khi rà soát báo cáo"));Check(mod.Tin.TrangThai=="DaAn"&&mod.LichSu.Count==2,"Approve then hide with reason and history");
            Guid circle;await using(var scope=f.Services.CreateAsyncScope())circle=await scope.ServiceProvider.GetRequiredService<DemoGiaoNhanSmartSwap>().KhoiTaoAsync();
            using var c=f.CreateClient();var cLogin=await Post<DangNhapDto>(c,"/api/taikhoan/dangnhap",new DangNhapRequest(DemoGiaoNhanSmartSwap.Emails[2],Environment.GetEnvironmentVariable("REMARKET_DEMO_PASSWORD")??"ReMarketDemo!2026"));c.DefaultRequestHeaders.Authorization=new("Bearer",cLogin.AccessToken);
            var shipping=(await c.GetFromJsonAsync<GiaoNhanSmartSwapDto>($"/api/smart-swaps/{circle}/delivery"))!;var incoming=shipping.VanChuyen.Single(x=>x.NguoiNhanId==cLogin.HoSo.Id);var circleDetail=(await c.GetFromJsonAsync<ChiTietVongSmartSwapDto>($"/api/smart-swaps/{circle}"))!;
            var gd=circleDetail.GiaoDichId;var adminTrade=(await admin.GetFromJsonAsync<ChiTietGiaoDichQuanTriDto>($"/api/admin/giao-dich/{gd}"))!;
            Check(adminTrade.GiaoDich.ThanhVien.Count==3&&adminTrade.GiaoDich.SanPham.Count==3&&adminTrade.VanChuyen.Count==3&&adminTrade.GiaoDich.SoNguoiNhan==2,"Admin sees real three members/products/shipments and2/3 receipts");
            await Status(await admin.PostAsJsonAsync($"/api/smart-swaps/{circle}/shipments/{incoming.Id}/received",new NhanHangSmartSwapRequest(shipping.PhienBanThoaThuanId!.Value,true,true,true)),404,"ADMIN cannot impersonate designated receiver via USER endpoint");
            await Status(await admin.PostAsJsonAsync($"/api/admin/giao-dich/{gd}/received",new{}),404,"No admin endpoint exists to confirm receipt");
            var image=adminTrade.GiaoDich.SanPham.First(x=>x.AnhUrl!=null).AnhUrl!;await Status(await admin.GetAsync(image),200,"Admin reads scoped actual product image");await Status(await user.GetAsync(image),403,"User denied admin image");
            var paused=await Post<GiaoNhanSmartSwapDto>(c,$"/api/smart-swaps/{circle}/issues",new BaoSuCoSmartSwapRequest(Guid.NewGuid(),incoming.Id,"SaiHang","Sản phẩm nhận chưa đúng","Đề nghị nhóm và quản trị kiểm tra sản phẩm khác mô tả."));
            var issue=paused.SuCo.Single();var complaint=(await admin.GetFromJsonAsync<ChiTietKhieuNaiQuanTriDto>($"/api/admin/khieu-nai/{issue.Id}"))!;
            complaint=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{issue.Id}/tiep-nhan",new TiepNhanKhieuNaiRequest(complaint.KhieuNai.PhienBan,"Tiếp nhận và yêu cầu các bên cung cấp thông tin."));
            var decision=new QuyetDinhQuanTriRequest(Guid.NewGuid(),complaint.KhieuNai.PhienBan,"Đã đối chiếu chat, sản phẩm và xác minh với các bên.","Các bên thống nhất tiếp tục","Bổ sung đúng món để thành viên tự kiểm tra khi nhận.","Đã rà soát, các bên thực hiện phương án bổ sung đúng hàng.","TiepTuc",true);
            complaint=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{issue.Id}/quyet-dinh",decision);await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{issue.Id}/quyet-dinh",decision);Check(complaint.QuyetDinh.Count==1&&complaint.KhieuNai.TrangThai=="ChoThucHien","Decision waits verification and retry does not duplicate");
            await Status(await admin.PostAsJsonAsync($"/api/admin/khieu-nai/{issue.Id}/quyet-dinh",decision with{KetLuan="Thay đổi trái phép cùng mã"}),409,"Decision key cannot be reused for different content");
            var receivedBefore=await Db(db=>db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==gd));
            await Status(await admin.PostAsJsonAsync($"/api/admin/khieu-nai/{issue.Id}/xac-minh",new XacMinhQuyetDinhRequest(complaint.KhieuNai.PhienBan,complaint.QuyetDinh[0].Id,false,"Chưa kiểm tra đầy đủ.")),400,"Cannot close before explicit execution verification");
            complaint=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{issue.Id}/xac-minh",new XacMinhQuyetDinhRequest(complaint.KhieuNai.PhienBan,complaint.QuyetDinh[0].Id,true,"Đã đối chiếu bằng chứng thực hiện và trao đổi với cả ba."));
            Check(complaint.KhieuNai.TrangThai=="DaGiaiQuyet"&&await Db(db=>db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==gd))==receivedBefore,"Resolving dispute does not create or change participant receipts");
            shipping=(await c.GetFromJsonAsync<GiaoNhanSmartSwapDto>($"/api/smart-swaps/{circle}/delivery"))!;Check(shipping.TienDo.SoNguoiNhanDung==2&&!shipping.TienDo.HoanTat&&!shipping.TienDo.TamDung,"Verified continuation restores workflow at2/3, not completed");
            var draft=new TaoKhieuNaiQuanTriRequest(Guid.NewGuid(),cLogin.HoSo.Id,adminTrade.GiaoDich.ThanhVien.First(x=>x.NguoiDungId!=cLogin.HoSo.Id).NguoiDungId,"Đối chiếu thêm phản ánh","Các bên đề nghị quản trị đối chiếu thông tin bổ sung trước khi tiếp tục.",true);
            await Status(await admin.PostAsJsonAsync($"/api/admin/giao-dich/{gd}/khieu-nai",draft with{NguoiBiKhieuNaiId=uid}),400,"Admin complaint must reference two actual transaction participants");
            var a=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/giao-dich/{gd}/khieu-nai",draft,201);var retry=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/giao-dich/{gd}/khieu-nai",draft,201);Check(a.KhieuNai.Id==retry.KhieuNai.Id,"Admin recorded complaint is idempotent and attributed in audit");
            a=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{a.KhieuNai.Id}/quyet-dinh",decision with{MaYeuCau=Guid.NewGuid(),PhienBan=a.KhieuNai.PhienBan,HuongXuLy="GhiNhan"});
            var holdVerification=new XacMinhQuyetDinhRequest(a.KhieuNai.PhienBan,a.QuyetDinh[0].Id,true,"Đã kiểm tra, cần tiếp tục bổ sung bằng chứng trước khi quyết định cuối.");
            a=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{a.KhieuNai.Id}/xac-minh",holdVerification);
            Check(a.KhieuNai.TrangThai=="DangXuLy"&&(await c.GetFromJsonAsync<GiaoNhanSmartSwapDto>($"/api/smart-swaps/{circle}/delivery"))!.TienDo.TamDung,"Verified temporary hold keeps complaint open for another decision");
            await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{a.KhieuNai.Id}/xac-minh",holdVerification);
            var b=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/giao-dich/{gd}/khieu-nai",draft with{MaYeuCau=Guid.NewGuid(),TieuDe="Phản ánh bổ sung thứ hai"},201);
            foreach(var item in new[]{a,b}){var result=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{item.KhieuNai.Id}/quyet-dinh",decision with{MaYeuCau=Guid.NewGuid(),PhienBan=item.KhieuNai.PhienBan});result=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{item.KhieuNai.Id}/xac-minh",new XacMinhQuyetDinhRequest(result.KhieuNai.PhienBan,result.QuyetDinh.Single(x=>!x.DaXacMinhThucHien).Id,true,"Đã rà soát nội dung và bằng chứng thực hiện của cả ba."));var progress=(await c.GetFromJsonAsync<GiaoNhanSmartSwapDto>($"/api/smart-swaps/{circle}/delivery"))!;Check(progress.TienDo.TamDung==(item==a),"Remain paused until all complaints resolved; receipts unchanged at2/3");Check(progress.TienDo.SoNguoiNhanDung==2&&!progress.TienDo.HoanTat,"Closing complaint never completes two-of-three receipts");}
            shipping=await Post<GiaoNhanSmartSwapDto>(c,$"/api/smart-swaps/{circle}/shipments/{incoming.Id}/received",new NhanHangSmartSwapRequest(shipping.PhienBanThoaThuanId!.Value,true,true,true));Check(shipping.TienDo.HoanTat,"Only real third receiver can finish after dispute resolution");
            Check(await Db(db=>db.NhatKyQuanTri.CountAsync(x=>x.NguoiThucHienId==aid))>=8,"Admin audit persisted across account/catalog/moderation/dispute actions");
            // A second independent official circle covers a verified return/cancellation without fabricating receipts.
            var actors=new[]{user,second,c};var newTins=new List<TinDangDto>();var titles=new[]{"Camera "+Guid.NewGuid().ToString("N"),"Laptop "+Guid.NewGuid().ToString("N"),"iPad "+Guid.NewGuid().ToString("N")};
            for(var i=0;i<3;i++){var item=await Post<TinDangDto>(actors[i],"/api/tindang",new LuuTinDangRequest(titles[i],parent.Id,500000+i*100000,"NhuMoi","Món đồ kiểm thử phương án hoàn trả và kết thúc tranh chấp.","Hà Nội","Hà Nội",null,new DieuKienTraoDoiRequest("Muốn nhận "+titles[(i+1)%3],true,true,true,200000)),201);byte[] photo;using(var bmap=new SKBitmap(80,80)){bmap.Erase(SKColors.Blue);photo=bmap.Encode(SKEncodedImageFormat.Png,100).ToArray();}using var form=new MultipartFormDataContent();form.Add(new StringContent(item.PhienBan),"phienBan");form.Add(new ByteArrayContent(photo),"anh","return.png");item=(await(await actors[i].PostAsync($"/api/tindang/{item.Id}/anh",form)).Content.ReadFromJsonAsync<TinDangDto>())!;item=await Post<TinDangDto>(actors[i],$"/api/tindang/{item.Id}/gui-duyet",new PhienBanTinRequest(item.PhienBan));newTins.Add((await Post<ChiTietTinQuanTriDto>(admin,$"/api/admin/san-pham/{item.Id}/duyet",new DuyetTinRequest(item.PhienBan,true,null))).Tin);}
            var searchJson=System.Text.Json.JsonDocument.Parse(await user.GetStringAsync($"/api/smart-swaps/search?tinDangId={newTins[0].Id}"));var match=searchJson.RootElement.GetProperty("items")[0].GetProperty("maDeXuat").GetString()!;
            var preview=await Post<XemVongSmartSwapDto>(user,"/api/smart-swaps/preview",new XemVongSmartSwapRequest(match));var returning=await Post<ChiTietVongSmartSwapDto>(user,"/api/smart-swaps/invitations",new GuiLoiMoiSmartSwapRequest(Guid.NewGuid(),match,preview.BamPhuongAn,"Cùng kiểm tra phương án hoàn trả khi có tranh chấp.",true),201);
            foreach(var actorClient in actors.Skip(1))returning=await Post<ChiTietVongSmartSwapDto>(actorClient,$"/api/smart-swaps/{returning.Id}/responses",new PhanHoiSmartSwapRequest(returning.PhuongAnHienTai!.Id,true,true));
            var returningComplaint=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/giao-dich/{returning.GiaoDichId}/khieu-nai",new TaoKhieuNaiQuanTriRequest(Guid.NewGuid(),uid,sid,"Các bên đề nghị hoàn trả","Đã đối chiếu các bên không tiếp tục và đề nghị kiểm tra hoàn trả.",true),201);
            returningComplaint=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{returningComplaint.KhieuNai.Id}/quyet-dinh",decision with{MaYeuCau=Guid.NewGuid(),PhienBan=returningComplaint.KhieuNai.PhienBan,HuongXuLy="KetThuc",KetLuan="Thống nhất kết thúc sau hoàn trả"});
            returningComplaint=await Post<ChiTietKhieuNaiQuanTriDto>(admin,$"/api/admin/khieu-nai/{returningComplaint.KhieuNai.Id}/xac-minh",new XacMinhQuyetDinhRequest(returningComplaint.KhieuNai.PhienBan,returningComplaint.QuyetDinh[0].Id,true,"Đã đối chiếu việc hoàn trả và xác minh với đủ các thành viên."));
            Check(await Db(db=>db.GiaoDich.AnyAsync(x=>x.Id==returning.GiaoDichId&&x.TrangThai==TrangThaiGiaoDich.DaHuy))&&await Db(db=>db.GiuChoSanPham.AllAsync(x=>x.GiaoDichId!=returning.GiaoDichId||x.DaGiaiPhong)),"Verified termination cancels and releases only the disputed transaction holds");
            Check(!await Db(db=>db.XacNhanGiaoNhan.AnyAsync(x=>x.GiaoDichId==returning.GiaoDichId))&&!await Db(db=>db.TienBu.AnyAsync(x=>x.GiaoDichId==returning.GiaoDichId&&(x.NguoiTraDaXacNhan||x.NguoiNhanDaXacNhan))),"Termination does not forge received goods or cash acknowledgements");
            var stats=(await admin.GetFromJsonAsync<BaoCaoQuanTriDto>("/api/admin/bao-cao"))!;Check(stats.BieuDo.Sum(x=>x.MuaBan)==stats.MuaBan&&stats.BieuDo.Sum(x=>x.TraoDoi)==stats.TraoDoi,"Report chart aggregates match SQL totals");
            await Change(async db=>{var role=await db.NguoiDungVaiTro.SingleAsync(x=>x.NguoiDungId==aid&&x.VaiTro.Ma=="ADMIN");db.Remove(role);});await Status(await admin.GetAsync("/api/admin/bao-cao"),403,"Revoking ADMIN role rejects existing JWT immediately");
            Console.WriteLine($"Admin HTTP/SQL checks: {count} passed. Real SQL, JWT roles, dispute verification and no admin receipt impersonation.");return 0;
        }catch(Exception ex){Console.Error.WriteLine("FAIL Admin: "+ex);return 1;}
        finally{if(f!=null)await f.DisposeAsync();if(created&&name.StartsWith("ReMarketTest_",StringComparison.Ordinal))await new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]",master).ExecuteNonQueryAsync();if(Directory.Exists(folder)&&folder.StartsWith(root,StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(folder)=="remarket-admin-"+name)Directory.Delete(folder,true);}
    }
}
