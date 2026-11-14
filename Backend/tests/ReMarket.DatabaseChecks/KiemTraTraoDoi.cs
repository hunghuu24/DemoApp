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
using ReMarket.Application.TaiKhoan;
using ReMarket.Application.TraoDoi;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;
using SkiaSharp;

namespace ReMarket.DatabaseChecks;

internal static class KiemTraTraoDoi
{
    private sealed class Factory(string connection,string folder):WebApplicationFactory<TraoDoiController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder b)=>b.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","src","ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi().ConfigureLogging(l=>l.ClearProviders()).ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>{
                ["ConnectionStrings:ReMarket"]=connection,["Jwt:SigningKey"]=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),["Media:ProductFolder"]=folder,["RateLimits:PermitLimit"]="10000",["Commerce:EnableExpirationWorker"]="false",["Exchange:EnableExpirationWorker"]="false"}));
    }
    public static async Task<int> ChayAsync()
    {
        var name="ReMarketTest_"+Guid.NewGuid().ToString("N");var folder=Path.GetFullPath(Path.Combine(Path.GetTempPath(),"remarket-exchange-"+name));
        var cs=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket")??"Server=localhost;Trusted_Connection=True;TrustServerCertificate=True"){InitialCatalog="master"};
        await using var master=new SqlConnection(cs.ConnectionString);await master.OpenAsync();Factory? factory=null;var created=false;var passed=0;
        void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);passed++;Console.WriteLine("PASS "+text);}
        async Task Status(HttpResponseMessage r,int expected,string text){if((int)r.StatusCode!=expected)throw new InvalidOperationException(text+": "+(int)r.StatusCode+" "+await r.Content.ReadAsStringAsync());Check(true,text+" ["+expected+"]");}
        try
        {
            await new SqlCommand($"CREATE DATABASE [{name}]",master).ExecuteNonQueryAsync();created=true;cs.InitialCatalog=name;factory=new Factory(cs.ConnectionString,folder);
            using var a=factory.CreateClient();using var b=factory.CreateClient();using var stranger=factory.CreateClient();using var admin=factory.CreateClient();using var anon=factory.CreateClient();
            async Task<T> Db<T>(Func<ReMarketDbContext,Task<T>> fn){await using var scope=factory.Services.CreateAsyncScope();return await fn(scope.ServiceProvider.GetRequiredService<ReMarketDbContext>());}
            await Db(async db=>{await db.Database.MigrateAsync();return true;});
            async Task<Guid> Account(HttpClient c,string label,string phone)
            {
                var email=label+name+"@example.test";var r=await c.PostAsJsonAsync("/api/taikhoan/dangky",new DangKyRequest(label,email,phone,"ExchangeTest!2026","ExchangeTest!2026",true));await Status(r,201,"Register real "+label);var user=(await r.Content.ReadFromJsonAsync<HoSoDto>())!;
                var login=await c.PostAsJsonAsync("/api/taikhoan/dangnhap",new DangNhapRequest(email,"ExchangeTest!2026"));await Status(login,200,"Login real "+label);c.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",(await login.Content.ReadFromJsonAsync<DangNhapDto>())!.AccessToken);return user.Id;
            }
            var aid=await Account(a,"MinhAnhDemo","0911111111");var bid=await Account(b,"HoangNamDemo","0922222222");var other=await Account(stranger,"Stranger","0933333333");var adminId=await Account(admin,"Moderator","0944444444");
            var category=new DanhMuc{Ten="Điện tử",Ma="exchange-check",DangHienThi=true};
            await Db(async db=>{db.Add(category);db.Add(new NguoiDungVaiTro{NguoiDungId=adminId,VaiTroId=await db.VaiTro.Where(x=>x.Ma=="ADMIN").Select(x=>x.Id).SingleAsync()});await db.SaveChangesAsync();return true;});
            using var bitmap=new SKBitmap(200,160);using(var canvas=new SKCanvas(bitmap))canvas.Clear(SKColors.SeaGreen);using var image=SKImage.FromBitmap(bitmap);using var encoded=image.Encode(SKEncodedImageFormat.Png,100);var png=encoded.ToArray();
            async Task<T> Post<T>(HttpClient client,string path,object body,int expected=200,string label="") {var r=await client.PostAsJsonAsync(path,body);await Status(r,expected,label.Length>0?label:path);return(await r.Content.ReadFromJsonAsync<T>())!;}
            LuuTinDangRequest Form(string title,bool mot=true,bool smart=true,bool bu=true,long max=1000000)=>new(title,category.Id,1200000,"NhuMoi","Sản phẩm còn hoạt động tốt, có thể kiểm tra khi gặp.","TP. Hồ Chí Minh","TP. Hồ Chí Minh",null,new("Muốn nhận đồ điện tử hoặc sản phẩm phù hợp.",mot,smart,bu,max));
            async Task<TinDangDto> Photo(HttpClient owner,TinDangDto tin,int expected=200)
            {
                using var body=new MultipartFormDataContent();body.Add(new StringContent(tin.PhienBan),"phienBan");body.Add(new ByteArrayContent(png),"anh","photo.png");var result=await owner.PostAsync($"/api/tindang/{tin.Id}/anh",body);await Status(result,expected,"Upload exchange picture");return expected==200?(await result.Content.ReadFromJsonAsync<TinDangDto>())!:tin;
            }
            async Task<TinDangDto> Publish(HttpClient owner,string title,bool mot=true,bool bu=true,long max=1000000)
            {
                var tin=await Post<TinDangDto>(owner,"/api/tindang",Form(title,mot,true,bu,max),201,"Create exchange draft");tin=await Photo(owner,tin);
                tin=await Post<TinDangDto>(owner,$"/api/tindang/{tin.Id}/gui-duyet",new PhienBanTinRequest(tin.PhienBan));return await Post<TinDangDto>(admin,$"/api/admin/tindang/{tin.Id}/duyet",new DuyetTinRequest(tin.PhienBan,true,null));
            }
            await Status(await anon.PostAsJsonAsync("/api/tindang",Form("Không xác thực")),401,"Anonymous cannot post exchange");
            await Status(await a.PostAsJsonAsync("/api/tindang",Form("Sai hình thức",false,false)),400,"At least one exchange mode required");
            await Status(await a.PostAsJsonAsync("/api/tindang",Form("Sai tiền bù",true,true,false,100)),400,"Disabled compensation cannot retain nonzero cap");
            var draft=await Post<TinDangDto>(a,"/api/tindang",Form("Bản nháp trao đổi"),201);Check(draft.LoaiTin=="TraoDoi"&&draft.GiaBan==null&&draft.TraoDoi?.ChoPhepMotMot==true,"Exchange value and needs persisted separately from sale price");
            await Status(await a.PostAsJsonAsync($"/api/tindang/{draft.Id}/gui-duyet",new PhienBanTinRequest(draft.PhienBan)),400,"Cannot publish without picture");
            for(var n=0;n<6;n++)draft=await Photo(a,draft);await Photo(a,draft,400);
            var drafts=(await a.GetFromJsonAsync<TrangTinDto>("/api/traodoi/moncuatoi"))!;Check(drafts.Items.Count==0,"Draft not selectable");
            var ta=await Publish(a,"Sony Minh Anh");var tb=await Publish(b,"Kindle Hoàng Nam");var tc=await Publish(a,"Đồ dự phòng");
            var smartOnly=await Publish(a,"Chỉ Smart Swap",false);Check(smartOnly.TraoDoi?.ChoPhepMotMot==false,"False 1-to-1 flag survives SQL default");
            var before=await Db(db=>db.GiaoDich.CountAsync());
            await Status(await a.GetAsync($"/api/tindang/{tb.Id}"),200,"View exchange detail");
            var discover=(await a.GetFromJsonAsync<TrangTinDto>("/api/tindang?loaiTin=TraoDoi&tuKhoa=hoang%20nam&choPhepBuTien=true"))!;Check(discover.Items.Single().Id==tb.Id,"Accent-insensitive exchange search and filter");
            var own=(await a.GetFromJsonAsync<TrangTinDto>("/api/traodoi/moncuatoi"))!;Check(own.Items.All(x=>x.NguoiDangId==aid&&x.TraoDoi!.ChoPhepMotMot)&&own.TongSo==2,"Selector owns items and excludes Smart-Swap-only listings");
            Check(before==await Db(db=>db.GiaoDich.CountAsync()),"Discovery/detail/selector GETs never create transactions");
            TaoDeNghiRequest Create(TinDangDto gui,TinDangDto nhan,long amount=300000,Guid? payer=null)=>new(Guid.NewGuid(),gui.Id,nhan.Id,gui.PhienBan,nhan.PhienBan,amount,payer,"Mình muốn đổi đồ và bù thêm khi gặp.","Nhà văn hóa, TP. Hồ Chí Minh",null,true);
            await Status(await a.PostAsJsonAsync("/api/traodoi",Create(tb,ta)),403,"Cannot offer another owner's item");
            await Status(await a.PostAsJsonAsync("/api/traodoi",Create(ta,tc)),400,"Cannot exchange with self");
            await Status(await a.PostAsJsonAsync("/api/traodoi",Create(draft,tb)),409,"Draft cannot enter offer");
            await Status(await a.PostAsJsonAsync("/api/traodoi",Create(smartOnly,tb)),409,"Smart-only listing cannot enter 1-to-1");
            await Status(await a.PostAsJsonAsync("/api/traodoi",Create(ta,tb,1000001)),400,"Payer cap enforced");
            var request=Create(ta,tb);var offer=await Post<DeNghiTraoDoiDto>(a,"/api/traodoi",request,201,"Send real offer");
            Check(offer.ThoaThuan?.SoTienBu==300000&&offer.ThoaThuan.DongY.Count(x=>x.QuyetDinh=="DongY")==1,"Offer creates version1 with sender consent only");
            Check(!await Db(db=>db.GiuChoSanPham.AnyAsync(x=>x.GiaoDichId==offer.GiaoDichId&&!x.DaGiaiPhong)),"Sending pending offer does not reserve stock");
            var replay=await Post<DeNghiTraoDoiDto>(a,"/api/traodoi",request,201,"Retry same creation");Check(replay.Id==offer.Id,"Creation key avoids duplicate transaction");
            await Status(await a.PostAsJsonAsync("/api/traodoi",request with{LoiNhan="Nội dung mới không cùng yêu cầu"}),409,"Creation key cannot change payload");
            await Status(await stranger.GetAsync($"/api/traodoi/{offer.Id}"),404,"Offer privacy");
            await Status(await a.PostAsJsonAsync($"/api/traodoi/{offer.Id}/phanhoi",new PhanHoiDeNghiRequest(offer.PhienBan,true)),403,"Creator cannot self-approve");
            await Status(await a.PostAsJsonAsync($"/api/traodoi/{offer.Id}/giaonhan",new GiaoNhanTraoDoiRequest(offer.PhienBan,true,true,true,true)),409,"Handover locked before mutual consent");
            var incoming=(await b.GetFromJsonAsync<TrangDeNghiDto>("/api/traodoi?tab=nhan"))!;Check(incoming.Items.Single().Id==offer.Id,"Receiver inbox uses authenticated identity");
            var staleVersion=offer.PhienBan;
            offer=await Post<DeNghiTraoDoiDto>(b,$"/api/traodoi/{offer.Id}/dexuatlai",new DoiDeNghiRequest(offer.PhienBan,400000,aid,"Mình mong bạn bù thêm 400.000đ nhé.","Nhà văn hóa",null));
            Check(offer.LichSu.Count==2&&offer.LichSu[0].SoTienBu==300000&&offer.ThoaThuan!.SoTienBu==400000&&offer.ThoaThuan.DongY.Single(x=>x.NguoiDungId==aid).QuyetDinh=="ChoPhanHoi","Counter preserves old terms and resets consent");
            await Status(await a.PostAsJsonAsync($"/api/traodoi/{offer.Id}/phanhoi",new PhanHoiDeNghiRequest(staleVersion,true)),409,"Stale agreement cannot be accepted");
            await Status(await b.PostAsJsonAsync($"/api/traodoi/{offer.Id}/phanhoi",new PhanHoiDeNghiRequest(offer.PhienBan,true)),403,"Counter creator cannot approve for partner");
            var chat=await Post<HoiThoaiDto>(a,$"/api/traodoi/{offer.Id}/hoithoai",new{});var chat2=await Post<HoiThoaiDto>(b,$"/api/traodoi/{offer.Id}/hoithoai",new{});Check(chat.Id==chat2.Id,"Private exchange chat reused for both parties");
            await Post<HoiThoaiDto>(a,$"/api/hoithoai/{chat.Id}/tinnhan",new GuiTinRequest(Guid.NewGuid(),"Mình đồng ý gặp để kiểm tra nhé."));await Status(await stranger.GetAsync($"/api/hoithoai/{chat.Id}"),404,"Stranger cannot enter exchange chat");
            offer=await Post<DeNghiTraoDoiDto>(a,$"/api/traodoi/{offer.Id}/phanhoi",new PhanHoiDeNghiRequest(offer.PhienBan,true));
            Check(offer.ThoaThuan!.TrangThai=="DaChot"&&offer.TrangThaiGiaoDich=="DangThucHien","Mutual consent finalizes exact version");
            Check(await Db(db=>db.GiuChoSanPham.CountAsync(x=>x.GiaoDichId==offer.GiaoDichId&&!x.DaGiaiPhong))==2,"Both items reserved atomically");
            Check(!(await a.GetFromJsonAsync<TrangTinDto>("/api/traodoi/moncuatoi"))!.Items.Any(x=>x.Id==ta.Id),"Held item disappears from selector");
            await Status(await anon.GetAsync($"/api/tindang/{tb.Id}"),404,"Held item withdrawn from public catalog");await Status(await a.GetAsync(offer.MonNhan.AnhUrl!),200,"Participant still sees held picture");await Status(await stranger.GetAsync(offer.MonNhan.AnhUrl!),404,"Private offer picture protected");
            var reCounter=await Post<DeNghiTraoDoiDto>(b,$"/api/traodoi/{offer.Id}/dexuatlai",new DoiDeNghiRequest(offer.PhienBan,350000,aid,"Điều chỉnh lại trước khi bàn giao.","Nhà văn hóa",null));
            Check(!reCounter.CoTheGiaoNhan&&reCounter.ThoaThuan!.DongY.Count(x=>x.QuyetDinh=="DongY")==1&&reCounter.TrangThaiGiaoDich=="ChoDongY","Counter after acceptance locks handover and resets both-side agreement");
            await Status(await a.PostAsJsonAsync($"/api/traodoi/{offer.Id}/giaonhan",new GiaoNhanTraoDoiRequest(reCounter.PhienBan,true,true,true,true)),409,"Cannot hand over superseded agreement");
            offer=await Post<DeNghiTraoDoiDto>(a,$"/api/traodoi/{offer.Id}/phanhoi",new PhanHoiDeNghiRequest(reCounter.PhienBan,true));
            var review=new DanhGiaTraoDoiRequest(Guid.NewGuid(),5,"Đối tác đúng hẹn, sản phẩm đúng mô tả.",[]);
            await Status(await a.PostAsJsonAsync($"/api/traodoi/{offer.Id}/danhgia",review),409,"No review before completion");
            offer=await Post<DeNghiTraoDoiDto>(a,$"/api/traodoi/{offer.Id}/giaonhan",new GiaoNhanTraoDoiRequest(offer.PhienBan,true,true,true,true));
            Check(offer.TrangThaiGiaoDich=="DangThucHien"&&offer.XacNhan.Count(x=>x.DaNhan)==1&&offer.ThoaThuan!.NguoiTraDaXacNhan&&!offer.ThoaThuan.NguoiNhanDaXacNhan,"First receipt waits for partner and only sets payer confirmation");
            await Status(await b.PostAsJsonAsync($"/api/traodoi/{offer.Id}/huy",new HuyDeNghiRequest(offer.PhienBan,"Không được hủy sau bàn giao")),409,"No cancellation after handover");
            await Status(await b.PostAsJsonAsync($"/api/traodoi/{offer.Id}/dexuatlai",new DoiDeNghiRequest(offer.PhienBan,0,null,"Không được thay đổi sau bàn giao",null,null)),409,"No counter after handover");
            var receiptRequest=new GiaoNhanTraoDoiRequest(offer.PhienBan,true,true,true,true);await Post<DeNghiTraoDoiDto>(a,$"/api/traodoi/{offer.Id}/giaonhan",receiptRequest);
            var proofKey=Guid.NewGuid();
            async Task UploadProof(HttpClient client,Guid key,int expected=200){using var form=new MultipartFormDataContent();form.Add(new StringContent(key.ToString()),"maYeuCau");form.Add(new ByteArrayContent(png),"anh","proof.png");await Status(await client.PostAsync($"/api/traodoi/{offer.Id}/bangchung",form),expected,"Upload handover evidence");}
            await UploadProof(a,proofKey);await UploadProof(a,proofKey);offer=(await a.GetFromJsonAsync<DeNghiTraoDoiDto>($"/api/traodoi/{offer.Id}"))!;Check(offer.BangChung.Count==1,"Evidence retry avoids duplicates");await Status(await a.GetAsync(offer.BangChung.Single().Url),200,"Participant reads actual handover proof");await Status(await stranger.GetAsync(offer.BangChung.Single().Url),404,"Evidence privacy enforced");await Status(await a.GetAsync("/api/traodoi/tieuchi"),200,"Rating criteria API accessible to participant");
            await UploadProof(b,Guid.NewGuid(),409);
            for(var imageIndex=1;imageIndex<5;imageIndex++)await UploadProof(a,Guid.NewGuid());await UploadProof(a,Guid.NewGuid(),400);
            offer=await Post<DeNghiTraoDoiDto>(b,$"/api/traodoi/{offer.Id}/giaonhan",new GiaoNhanTraoDoiRequest(offer.PhienBan,true,true,true,true));Check(offer.TrangThaiGiaoDich=="HoanTat"&&offer.XacNhan.All(x=>x.DaNhan)&&offer.ThoaThuan!.NguoiNhanDaXacNhan,"Two receipts and compensation confirmations complete transaction");
            Check(await Db(db=>db.TinDang.CountAsync(x=>(x.Id==ta.Id||x.Id==tb.Id)&&x.TrangThai==TrangThaiTinDang.DaTraoDoi))==2&&!await Db(db=>db.GiuChoSanPham.AnyAsync(x=>x.GiaoDichId==offer.GiaoDichId&&!x.DaGiaiPhong)),"Completed items marked exchanged and holds released");
            offer=await Post<DeNghiTraoDoiDto>(a,$"/api/traodoi/{offer.Id}/danhgia",review);await Post<DeNghiTraoDoiDto>(a,$"/api/traodoi/{offer.Id}/danhgia",review);await Status(await a.PostAsJsonAsync($"/api/traodoi/{offer.Id}/danhgia",review with{MaYeuCau=Guid.NewGuid()}),409,"Duplicate review rejected");
            offer=await Post<DeNghiTraoDoiDto>(b,$"/api/traodoi/{offer.Id}/danhgia",review with{MaYeuCau=Guid.NewGuid()});Check(offer.DanhGia.Count==2,"Both partners may rate exactly once");
            var t2b=await Publish(b,"Món để từ chối");tc=(await a.GetFromJsonAsync<TinDangDto>($"/api/tindang/{tc.Id}"))!;
            var reject=await Post<DeNghiTraoDoiDto>(a,"/api/traodoi",Create(tc,t2b,0),201);reject=await Post<DeNghiTraoDoiDto>(b,$"/api/traodoi/{reject.Id}/phanhoi",new PhanHoiDeNghiRequest(reject.PhienBan,false,"Mình chưa phù hợp món này"));Check(reject.TrangThai=="TuChoi","Decline persists without reserving items");
            var cancel=await Post<DeNghiTraoDoiDto>(a,"/api/traodoi",Create(tc,t2b,0),201);cancel=await Post<DeNghiTraoDoiDto>(b,$"/api/traodoi/{cancel.Id}/phanhoi",new PhanHoiDeNghiRequest(cancel.PhienBan,true));cancel=await Post<DeNghiTraoDoiDto>(a,$"/api/traodoi/{cancel.Id}/huy",new HuyDeNghiRequest(cancel.PhienBan,"Hủy trước khi bàn giao"));Check(cancel.TrangThai=="DaHuy"&&!await Db(db=>db.GiuChoSanPham.AnyAsync(x=>x.GiaoDichId==cancel.GiaoDichId&&!x.DaGiaiPhong)),"Cancel accepted exchange releases both reservations");
            tc=(await a.GetFromJsonAsync<TinDangDto>($"/api/tindang/{tc.Id}"))!;t2b=(await b.GetFromJsonAsync<TinDangDto>($"/api/tindang/{t2b.Id}"))!;
            var expiry=await Post<DeNghiTraoDoiDto>(a,"/api/traodoi",Create(tc,t2b,0),201);
            await Db(async db=>{await db.DeNghiTraoDoi.Where(x=>x.Id==expiry.Id).ExecuteUpdateAsync(s=>s.SetProperty(p=>p.NgayTao,DateTimeOffset.UtcNow.AddDays(-2)).SetProperty(p=>p.NgayHetHan,DateTimeOffset.UtcNow.AddDays(-1)));return true;});
            expiry=(await a.GetFromJsonAsync<DeNghiTraoDoiDto>($"/api/traodoi/{expiry.Id}"))!;Check(expiry.TrangThai=="HetHan"&&expiry.TrangThaiGiaoDich=="DaHuy","Expired pending offer persists cancellation");
            var outsiderTin=await Publish(stranger,"Món cạnh tranh");
            var competingA=await Post<DeNghiTraoDoiDto>(a,"/api/traodoi",Create(tc,t2b,0),201);
            var competingOther=await Post<DeNghiTraoDoiDto>(stranger,"/api/traodoi",Create(outsiderTin,t2b,0),201);
            var races=await Task.WhenAll(b.PostAsJsonAsync($"/api/traodoi/{competingA.Id}/phanhoi",new PhanHoiDeNghiRequest(competingA.PhienBan,true)),b.PostAsJsonAsync($"/api/traodoi/{competingOther.Id}/phanhoi",new PhanHoiDeNghiRequest(competingOther.PhienBan,true)));
            Check(races.Count(r=>(int)r.StatusCode==200)==1&&races.Count(r=>(int)r.StatusCode==409)==1,"Competing exchanges cannot reserve the same product twice");
            var winner=(await races.Single(r=>(int)r.StatusCode==200).Content.ReadFromJsonAsync<DeNghiTraoDoiDto>())!;
            await Post<DeNghiTraoDoiDto>(b,$"/api/traodoi/{winner.Id}/huy",new HuyDeNghiRequest(winner.PhienBan,"Hủy giao dịch kiểm tra cạnh tranh"));
            var pending=races[0].IsSuccessStatusCode?competingOther:competingA;
            await Db(async db=>{await db.GiaoDich.Where(x=>x.Id==pending.GiaoDichId).ExecuteUpdateAsync(s=>s.SetProperty(p=>p.TrangThai,TrangThaiGiaoDich.TranhChap));await db.DeNghiTraoDoi.Where(x=>x.Id==pending.Id).ExecuteUpdateAsync(s=>s.SetProperty(p=>p.NgayTao,DateTimeOffset.UtcNow.AddDays(-2)).SetProperty(p=>p.NgayHetHan,DateTimeOffset.UtcNow.AddDays(-1)));return true;});
            await Status(await b.PostAsJsonAsync($"/api/traodoi/{pending.Id}/huy",new HuyDeNghiRequest(pending.PhienBan,"Không được hủy tranh chấp")),409,"Disputed transaction cannot be cancelled");
            var disputed=(await b.GetFromJsonAsync<DeNghiTraoDoiDto>($"/api/traodoi/{pending.Id}"))!;
            Check(disputed.TrangThaiGiaoDich=="TranhChap"&&!disputed.CoTheHuy&&!disputed.CoTheDeXuatLai,"Reading expired legacy dispute preserves dispute state");
            Console.WriteLine($"Exchange HTTP/SQL checks: {passed} passed. Two demo participants, real JWT/SQL. No credentials printed.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine("Exchange checks failed: "+ex);return 1;}
        finally
        {
            if(factory!=null)await factory.DisposeAsync();if(created)await new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]",master).ExecuteNonQueryAsync();
            if(Directory.Exists(folder)&&folder.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(folder)=="remarket-exchange-"+name)Directory.Delete(folder,true);
        }
    }
}
