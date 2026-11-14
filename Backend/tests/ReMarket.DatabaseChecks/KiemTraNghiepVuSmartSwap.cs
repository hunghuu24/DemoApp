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
using ReMarket.Application.TraoDoi;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;
using ReMarket.Infrastructure.SmartSwap;
using SkiaSharp;

namespace ReMarket.DatabaseChecks;

internal static class KiemTraNghiepVuSmartSwap
{
    private sealed class Factory(string connection,string folder):WebApplicationFactory<VongSmartSwapController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder b)=>b.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","src","ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi().ConfigureLogging(x=>x.ClearProviders()).ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>{
                ["ConnectionStrings:ReMarket"]=connection,["Jwt:SigningKey"]=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),["Media:ProductFolder"]=folder,
                ["RateLimits:PermitLimit"]="10000",["Commerce:EnableExpirationWorker"]="false",["Exchange:EnableExpirationWorker"]="false",["SmartSwap:EnableExpirationWorker"]="false"}));
    }
    public static async Task<int> ChayAsync()
    {
        var name="ReMarketTest_"+Guid.NewGuid().ToString("N");var tempRoot=Path.GetFullPath(Path.GetTempPath());var folder=Path.GetFullPath(Path.Combine(tempRoot,"remarket-smart-workflow-"+name));
        var cs=new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket")??"Server=localhost;Trusted_Connection=True;TrustServerCertificate=True"){InitialCatalog="master"};
        await using var master=new SqlConnection(cs.ConnectionString);await master.OpenAsync();Factory? factory=null;var created=false;var passed=0;
        void Check(bool ok,string text){if(!ok)throw new InvalidOperationException(text);passed++;Console.WriteLine("PASS "+text);}
        async Task Status(HttpResponseMessage response,int expected,string text)
        {if((int)response.StatusCode!=expected)throw new InvalidOperationException(text+": "+(int)response.StatusCode+" "+await response.Content.ReadAsStringAsync());Check(true,text+" ["+expected+"]");}
        async Task<T> Post<T>(HttpClient client,string path,object body,int code=200)
        {using var response=await client.PostAsJsonAsync(path,body);await Status(response,code,"POST "+path);return(await response.Content.ReadFromJsonAsync<T>())!;}
        try
        {
            await new SqlCommand($"CREATE DATABASE [{name}]",master).ExecuteNonQueryAsync();created=true;cs.InitialCatalog=name;factory=new Factory(cs.ConnectionString,folder);
            using var a=factory.CreateClient();using var b=factory.CreateClient();using var c=factory.CreateClient();using var other=factory.CreateClient();using var admin=factory.CreateClient();using var anonymous=factory.CreateClient();
            async Task<T> Db<T>(Func<ReMarketDbContext,Task<T>> fn){await using var scope=factory.Services.CreateAsyncScope();return await fn(scope.ServiceProvider.GetRequiredService<ReMarketDbContext>());}
            async Task Change(Func<ReMarketDbContext,Task> fn)=>await Db(async db=>{await fn(db);await db.SaveChangesAsync();return true;});
            await Db(async db=>{await db.Database.MigrateAsync();Check(!db.Database.HasPendingModelChanges(),"Workflow model matches migration9");return true;});
            async Task<Guid> Account(HttpClient client,string label,string phone)
            {
                var email=label+name+"@example.test";var profile=await Post<HoSoDto>(client,"/api/taikhoan/dangky",new DangKyRequest(label,email,phone,"SmartFlowTest!2026","SmartFlowTest!2026",true),201);
                var login=await Post<DangNhapDto>(client,"/api/taikhoan/dangnhap",new DangNhapRequest(email,"SmartFlowTest!2026"));client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",login.AccessToken);return profile.Id;
            }
            var aid=await Account(a,"MemberA","0911111111");var bid=await Account(b,"MemberB","0922222222");var cid=await Account(c,"MemberC","0933333333");var did=await Account(other,"Outsider","0944444444");var adminId=await Account(admin,"Moderator","0955555555");
            var cat=new DanhMuc{Ten="Điện tử",Ma="smart-flow",DangHienThi=true};
            await Change(async db=>{db.Add(cat);db.Add(new NguoiDungVaiTro{NguoiDungId=adminId,VaiTroId=await db.VaiTro.Where(x=>x.Ma=="ADMIN").Select(x=>x.Id).SingleAsync()});});
            byte[] png;using(var bitmap=new SKBitmap(100,80)){bitmap.Erase(SKColors.Green);using var data=bitmap.Encode(SKEncodedImageFormat.Png,100);png=data.ToArray();}
            async Task<TinDangDto> Publish(HttpClient client,string title,string want,long value,long cap=2_000_000)
            {
                var form=new LuuTinDangRequest(title,cat.Id,value,"NhuMoi","Món đồ còn tốt, đủ phụ kiện và được kiểm tra khi gặp.","Hà Nội","Địa chỉ demo riêng tư",null,
                    new("Muốn nhận "+want,true,true,true,cap,null,new(cat.Id,want,"DaSuDungTot",0,20_000_000)));
                var tin=await Post<TinDangDto>(client,"/api/tindang",form,201);
                using var upload=new MultipartFormDataContent();upload.Add(new StringContent(tin.PhienBan),"phienBan");upload.Add(new ByteArrayContent(png),"anh","demo.png");
                using var result=await client.PostAsync($"/api/tindang/{tin.Id}/anh",upload);await Status(result,200,"Upload real listing photo");tin=(await result.Content.ReadFromJsonAsync<TinDangDto>())!;
                tin=await Post<TinDangDto>(client,$"/api/tindang/{tin.Id}/gui-duyet",new PhienBanTinRequest(tin.PhienBan));
                return await Post<TinDangDto>(admin,$"/api/admin/tindang/{tin.Id}/duyet",new DuyetTinRequest(tin.PhienBan,true,null));
            }
            var ta=await Publish(a,"iPhone13","Laptop",10_000_000);var tb=await Publish(b,"Laptop ThinkPad","iPad",12_000_000,0);var tc=await Publish(c,"iPad Air","iPhone",8_000_000);
            async Task<ChiTietVongSmartSwapDto> Get(Guid id,HttpClient? client=null)=>(await (client??a).GetFromJsonAsync<ChiTietVongSmartSwapDto>($"/api/smart-swaps/{id}"))!;
            async Task<int> Holds(Guid gd)=>await Db(db=>db.GiuChoSanPham.CountAsync(x=>x.GiaoDichId==gd&&!x.DaGiaiPhong));
            async Task<XemVongSmartSwapDto> Preview(params TinDangDto[] products)
            {
                if(products.Length==0)products=[ta,tb,tc];var ids=products.Select(x=>x.Id).ToHashSet();
                var found=(await a.GetFromJsonAsync<KetQuaTimSmartSwapDto>("/api/smart-swaps/search?tinDangId="+products[0].Id))!;
                var selected=found.Items.Single(x=>x.Chang.Select(l=>l.SanPham.TinDangId).ToHashSet().SetEquals(ids));
                return await Post<XemVongSmartSwapDto>(a,"/api/smart-swaps/preview",new XemVongSmartSwapRequest(selected.MaDeXuat));
            }
            GuiLoiMoiSmartSwapRequest InviteBody(XemVongSmartSwapDto preview)=>new(Guid.NewGuid(),preview.Vong.MaDeXuat,preview.BamPhuongAn,"Mời hai bạn trao đổi và kiểm tra phương án tiền bù.",true);
            async Task<ChiTietVongSmartSwapDto> Invite(params TinDangDto[] products)=>await Post<ChiTietVongSmartSwapDto>(a,"/api/smart-swaps/invitations",InviteBody(await Preview(products)),201);
            async Task<ChiTietVongSmartSwapDto> Vote(ChiTietVongSmartSwapDto v,HttpClient user)=>await Post<ChiTietVongSmartSwapDto>(user,$"/api/smart-swaps/{v.Id}/responses",new PhanHoiSmartSwapRequest(v.PhuongAnHienTai!.Id,true,true));
            async Task Cancel(ChiTietVongSmartSwapDto v)
            {v=await Get(v.Id);if(v.TrangThai!="DaHuy")await Post<ChiTietVongSmartSwapDto>(a,$"/api/smart-swaps/{v.Id}/cancel",new HuyVongSmartSwapRequest(v.PhuongAnHienTai!.Id,"Hủy vòng thử nghiệm trước khi giao nhận."));}
            async Task<ChiTietVongSmartSwapDto> Ready(ChiTietVongSmartSwapDto v){v=await Vote(v,b);return await Vote(v,c);}
            async Task Past(Guid id)=>await Change(async db=>{var row=await db.VongSmartSwap.SingleAsync(x=>x.Id==id);row.NgayTao=DateTimeOffset.UtcNow.AddDays(-2);row.NgayHetHan=DateTimeOffset.UtcNow.AddDays(-1);});
            async Task Worker(){await using var scope=factory.Services.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<VongSmartSwapService>().HetHanAsync(CancellationToken.None);}

            await Status(await anonymous.GetAsync("/api/smart-swaps"),401,"Anonymous cannot list circles");
            var before=await Db(db=>db.GiaoDich.CountAsync());var preview=await Preview();
            Check(preview.HoSo.Count==3&&preview.CoTheKhoiTao&&preview.Vong.TienBu.Sum(x=>x.SoTien)==4_000_000,"Preview shows three real profiles and proposed net cash");
            Check(before==await Db(db=>db.GiaoDich.CountAsync()),"Selecting and previewing do not create transactions");
            await Status(await a.PostAsJsonAsync("/api/smart-swaps/preview",new XemVongSmartSwapRequest(new string('x',98))),400,"Malformed proposal key rejected");
            await Status(await other.PostAsJsonAsync("/api/smart-swaps/invitations",InviteBody(preview)),403,"Nonparticipant cannot initiate circle");
            await Status(await a.PostAsJsonAsync("/api/smart-swaps/invitations",InviteBody(preview) with{DongYPhuongAn=false}),400,"Initiator must explicitly confirm initial plan");
            await Status(await a.PostAsJsonAsync("/api/smart-swaps/invitations",InviteBody(preview) with{BamPhuongAn=new string('0',64)}),409,"Forged preview hash rejected");
            await Change(async db=>{var row=await db.TinDang.SingleAsync(x=>x.Id==ta.Id);row.NgayCapNhat=DateTimeOffset.UtcNow;db.Entry(row).Property(x=>x.NgayCapNhat).IsModified=true;});
            await Status(await a.PostAsJsonAsync("/api/smart-swaps/invitations",InviteBody(preview)),409,"Changed listing version invalidates prior preview");
            preview=await Preview();var body=InviteBody(preview);
            var retried=await Task.WhenAll(a.PostAsJsonAsync("/api/smart-swaps/invitations",body),a.PostAsJsonAsync("/api/smart-swaps/invitations",body));
            foreach(var response in retried)await Status(response,201,"Concurrent invitation retry");
            var v=(await retried[0].Content.ReadFromJsonAsync<ChiTietVongSmartSwapDto>())!;var duplicate=(await retried[1].Content.ReadFromJsonAsync<ChiTietVongSmartSwapDto>())!;
            Check(v.Id==duplicate.Id&&await Db(db=>db.VongSmartSwap.CountAsync())==1,"Concurrent retry creates exactly one invitation and circle");
            Check((await a.GetFromJsonAsync<List<PhuongAnSmartSwapDto>>($"/api/smart-swaps/{v.Id}/versions"))!.Count==1,"Participant version-history endpoint contains initial agreement");
            Check(v.TrangThaiGiaoDich=="ChoDongY"&&v.PhuongAnHienTai!.DongY.Count(x=>x.QuyetDinh=="DongY")==1&&await Holds(v.GiaoDichId)==0,"Invitation is pending with creator vote only and no inventory hold");
            await Status(await a.PostAsJsonAsync("/api/smart-swaps/invitations",body with{LoiNhan="Nội dung khác dùng lại mã yêu cầu."}),409,"Invitation key cannot be reused with changed payload");
            await Status(await b.PostAsJsonAsync("/api/smart-swaps/invitations",InviteBody(preview)),409,"Duplicate pending directed circle rejected");
            foreach(var path in new[]{"", "/chat","/versions"})await Status(await other.GetAsync($"/api/smart-swaps/{v.Id}"+path),404,"Private circle resource protected");
            await Status(await other.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/responses",new PhanHoiSmartSwapRequest(v.PhuongAnHienTai!.Id,true,true)),404,"Outsider cannot vote");
            var notices=(await b.GetFromJsonAsync<List<ThongBaoSmartSwapDto>>("/api/smart-swaps/notifications?chuaDoc=true"))!;
            Check(notices.Single().VongId==v.Id&&(await c.GetFromJsonAsync<List<ThongBaoSmartSwapDto>>("/api/smart-swaps/notifications?chuaDoc=true"))!.Count==1,"Both invitees receive stored actionable notifications");
            await Status(await a.PostAsync($"/api/smart-swaps/notifications/{notices[0].Id}/read",null),404,"Cannot mark another user's notification");
            await Status(await b.PostAsync($"/api/smart-swaps/notifications/{notices[0].Id}/read",null),204,"Notification read owned");
            await Status(await b.PostAsync($"/api/smart-swaps/notifications/{notices[0].Id}/read",null),204,"Read notification is idempotent");
            var chat=(await b.GetFromJsonAsync<HoiThoaiDto>($"/api/smart-swaps/{v.Id}/chat"))!;
            Check(chat.Id==v.HoiThoaiId&&chat.TinNhan.Single().PhienBanThoaThuanId==v.PhuongAnHienTai!.Id,"Group chat has linked structured version1 message");
            foreach(var user in new[]{a,b,c})chat=await Post<HoiThoaiDto>(user,$"/api/hoithoai/{chat.Id}/tinnhan",new GuiTinRequest(Guid.NewGuid(),"Cùng thương lượng phương án tiền bù trong nhóm nhé."));
            Check(chat.TinNhan.Count(x=>x.Loai=="VanBan")==3,"All three members can negotiate in the same real group chat");
            await Status(await other.GetAsync($"/api/hoithoai/{chat.Id}"),404,"Group chat membership enforced");
            await Status(await other.PostAsJsonAsync($"/api/hoithoai/{chat.Id}/tinnhan",new GuiTinRequest(Guid.NewGuid(),"Tôi không phải thành viên.")),404,"Outsider cannot send group messages");
            await Status(await b.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/responses",new PhanHoiSmartSwapRequest(v.PhuongAnHienTai!.Id,true,false)),400,"Consent must explicitly acknowledge the entire plan");
            v=await Vote(v,b);Check(v.PhuongAnHienTai!.DongY.Count(x=>x.QuyetDinh=="DongY")==2&&await Holds(v.GiaoDichId)==0&&!v.CoTheBanGiao,"Two of three consents cannot start official transaction");
            await Db(async db=>
            {
                foreach(var sql in new FormattableString[]{$"UPDATE VongSmartSwap SET TrangThai='SanSangBanGiao' WHERE Id={v.Id}",$"UPDATE PhienBanThoaThuan SET TrangThai='DaChot' WHERE Id={v.PhuongAnHienTai.Id}"})
                {await using var tx=await db.Database.BeginTransactionAsync();var rejected=false;try{await db.Database.ExecuteSqlInterpolatedAsync(sql);}catch(SqlException ex)when(ex.Number is 51001 or 51003){rejected=true;}finally{await tx.RollbackAsync();}Check(rejected,"SQL trigger blocks premature ready/closed state");}
                return true;
            });
            v=await Vote(v,c);Check(v.TrangThai=="SanSangBanGiao"&&v.TrangThaiGiaoDich=="DangThucHien"&&v.CoTheBanGiao&&await Holds(v.GiaoDichId)==3,"Third consent atomically starts official transaction and holds all three products");
            var votesBefore=await Db(db=>db.DongYThoaThuan.CountAsync());v=await Vote(v,c);Check(votesBefore==await Db(db=>db.DongYThoaThuan.CountAsync()),"Repeated consent does not duplicate votes");
            await Status(await a.GetAsync($"/api/tindang/{ta.Id}"),404,"Held product removed from public catalog");
            await Status(await a.GetAsync(v.ThanhVien[0].AnhUrl!),200,"Participant can read private held product image");await Status(await other.GetAsync(v.ThanhVien[0].AnhUrl!),404,"Private circle image ownership enforced");
            await Status(await a.GetAsync($"/api/smart-swaps/{v.Id}/products/{Guid.NewGuid()}/image"),404,"Foreign product not a circle image");

            var initial=v.PhuongAnHienTai!;
            List<KhoanBuSmartSwapRequest> Plan(PhuongAnSmartSwapDto p,Guid payer,long value)=>p.TienBu.Select(x=>new KhoanBuSmartSwapRequest(x.NguoiTraId,x.NguoiNhanId,x.NguoiTraId==payer?value:x.SoTien)).ToList();
            var request=new TaoPhuongAnSmartSwapRequest(Guid.NewGuid(),initial.Id,Plan(initial,aid,1_500_000),"Tôi đề xuất giảm phần bù của mình xuống1,5 triệu.");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/proposals",request with{TienBu=Plan(initial,cid,1_500_000)}),403,"Cannot modify payment solely between the other two members");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/proposals",request with{TienBu=Plan(initial,aid,2_000_001)}),400,"Negotiated aggregate payer cap enforced");
            await Status(await other.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/proposals",request),404,"Outsider cannot propose plan");
            v=await Post<ChiTietVongSmartSwapDto>(a,$"/api/smart-swaps/{v.Id}/proposals",request);
            Check(v.TrangThai=="ChoDongY"&&v.TrangThaiGiaoDich=="ChoDongY"&&v.PhuongAnHienTai!.DongY.All(x=>x.QuyetDinh=="ChoPhanHoi")&&!v.CoTheBanGiao,"New version resets ALL three consents, including proposer, and blocks handover");
            Check(await Holds(v.GiaoDichId)==3&&v.LichSu.Count==2&&v.LichSu[0].TrangThai=="DaThayThe"&&v.LichSu[0].TongTienTra==4_000_000&&v.LichSu[0].DongY.All(x=>x.QuyetDinh=="DongY"),"Counter keeps inventory and immutable old amounts/three consents in history");
            Check(v.PhuongAnHienTai!.TongTienTra==3_500_000&&v.PhuongAnHienTai.TongTienTra==v.PhuongAnHienTai.TongTienNhan,"Negotiated total outgoing equals incoming");
            var version2=v.PhuongAnHienTai.Id;var replay=await Post<ChiTietVongSmartSwapDto>(a,$"/api/smart-swaps/{v.Id}/proposals",request);Check(replay.LichSu.Count==2&&replay.PhuongAnHienTai!.Id==version2,"Proposal retry keeps one version and linked message");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/proposals",request with{LoiNhan="Nội dung khác dùng lại cùng mã phương án."}),409,"Proposal idempotency key cannot change payload");
            await Status(await b.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/responses",new PhanHoiSmartSwapRequest(initial.Id,true,true)),409,"Consent to superseded version rejected");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/proposals",request with{MaYeuCau=Guid.NewGuid()}),409,"Stale base version cannot replace a newer proposal");
            v=await Vote(v,a);v=await Vote(v,b);Check(!v.CoTheBanGiao&&v.PhuongAnHienTai!.DongY.Count(x=>x.QuyetDinh=="DongY")==2,"Counter still needs third fresh consent");v=await Vote(v,c);Check(v.CoTheBanGiao&&await Holds(v.GiaoDichId)==3,"Three fresh consents reactivate without duplicate holds");
            chat=(await c.GetFromJsonAsync<HoiThoaiDto>($"/api/smart-swaps/{v.Id}/chat"))!;Check(chat.TinNhan.Count(x=>x.PhienBanThoaThuanId!=null)==2,"One structured chat message per version, no retry duplicates");
            v=await Post<ChiTietVongSmartSwapDto>(c,$"/api/smart-swaps/{v.Id}/proposals",new TaoPhuongAnSmartSwapRequest(Guid.NewGuid(),v.PhuongAnHienTai!.Id,Plan(v.PhuongAnHienTai,cid,1_800_000),"Tôi cũng đề xuất giảm khoản bù của mình."));
            v=await Post<ChiTietVongSmartSwapDto>(b,$"/api/smart-swaps/{v.Id}/proposals",new TaoPhuongAnSmartSwapRequest(Guid.NewGuid(),v.PhuongAnHienTai!.Id,Plan(v.PhuongAnHienTai,aid,1_400_000),"Tôi đồng ý đề xuất lại mức tiền mình nhận."));
            Check(v.LichSu.Count==4&&v.PhuongAnHienTai!.DongY.All(x=>x.QuyetDinh=="ChoPhanHoi"),"Every participant can propose related cash changes; each version resets consensus");
            v=await Post<ChiTietVongSmartSwapDto>(b,$"/api/smart-swaps/{v.Id}/responses",new PhanHoiSmartSwapRequest(v.PhuongAnHienTai!.Id,false,false,"Tôi không đồng ý phương án mới này."));
            Check(v.MaKetThuc=="TuChoi"&&v.TrangThaiGiaoDich=="DaHuy"&&await Holds(v.GiaoDichId)==0,"One rejection cancels whole pending counter and releases all owned holds");
            await Status(await c.PostAsJsonAsync($"/api/smart-swaps/{v.Id}/responses",new PhanHoiSmartSwapRequest(v.PhuongAnHienTai!.Id,true,true)),409,"Rejected circle cannot continue");
            await Status(await a.GetAsync($"/api/tindang/{ta.Id}"),200,"Released product is public again");
            var received=(await b.GetFromJsonAsync<TrangVongSmartSwapDto>("/api/smart-swaps?tab=nhan"))!;Check(received.Items.Any(x=>x.Id==v.Id),"Invitee list includes received circles and final state");

            var expired=await Invite();await Past(expired.Id);
            await Status(await c.PostAsJsonAsync($"/api/smart-swaps/{expired.Id}/responses",new PhanHoiSmartSwapRequest(expired.PhuongAnHienTai!.Id,true,true)),409,"Expired last vote cannot revive circle");
            expired=await Get(expired.Id);Check(expired.MaKetThuc=="HetHan"&&expired.TrangThaiGiaoDich=="DaHuy","Expiry is persisted even when mutation returns409");
            var timedCounter=await Ready(await Invite());timedCounter=await Post<ChiTietVongSmartSwapDto>(a,$"/api/smart-swaps/{timedCounter.Id}/proposals",new TaoPhuongAnSmartSwapRequest(Guid.NewGuid(),timedCounter.PhuongAnHienTai!.Id,Plan(timedCounter.PhuongAnHienTai,aid,1_700_000),"Đề nghị mức bù mới cần xác nhận trong thời hạn."));
            await Past(timedCounter.Id);await Worker();timedCounter=await Get(timedCounter.Id);Check(timedCounter.MaKetThuc=="HetHan"&&await Holds(timedCounter.GiaoDichId)==0,"Expiration worker cancels counter and restores three products");
            var cancel=await Ready(await Invite());await Cancel(cancel);Check(await Holds(cancel.GiaoDichId)==0,"Explicit cancel releases all three products before handover");
            var disputed=await Invite();await Change(async db=>(await db.GiaoDich.SingleAsync(x=>x.Id==disputed.GiaoDichId)).TrangThai=TrangThaiGiaoDich.TranhChap);await Past(disputed.Id);await Worker();
            Check((await Get(disputed.Id)).TrangThaiGiaoDich=="TranhChap","Expiry and reads preserve a disputed transaction");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{disputed.Id}/cancel",new HuyVongSmartSwapRequest(disputed.PhuongAnHienTai!.Id,"Không được xóa trạng thái tranh chấp.")),409,"Cannot cancel disputed transaction");
            await Change(async db=>{(await db.GiaoDich.SingleAsync(x=>x.Id==disputed.GiaoDichId)).TrangThai=TrangThaiGiaoDich.ChoDongY;(await db.VongSmartSwap.SingleAsync(x=>x.Id==disputed.Id)).NgayHetHan=DateTimeOffset.UtcNow.AddHours(1);});await Cancel(disputed);

            var hidden=await Vote(await Invite(),b);var hiddenTin=(await a.GetFromJsonAsync<TinDangDto>($"/api/tindang/{ta.Id}/quanly"))!;
            await Post<TinDangDto>(a,$"/api/tindang/{ta.Id}/an",new PhienBanTinRequest(hiddenTin.PhienBan));
            await Status(await c.PostAsJsonAsync($"/api/smart-swaps/{hidden.Id}/responses",new PhanHoiSmartSwapRequest(hidden.PhuongAnHienTai!.Id,true,true)),409,"Final acceptance rechecks product withdrawn after invitation");
            Check(await Holds(hidden.GiaoDichId)==0,"Unavailable product causes no partial reservation");await Cancel(hidden);
            await Status(await a.GetAsync($"/api/tindang/{ta.Id}"),404,"Cancelling unreserved invitation does not unhide withdrawn product");
            await Change(async db=>(await db.TinDang.SingleAsync(x=>x.Id==ta.Id)).TrangThai=TrangThaiTinDang.DangHienThi);
            var handed=await Ready(await Invite());var delivered=handed.Chang.Single(x=>x.NguoiGiaoId==aid);
            var receipt=new XacNhanGiaoNhan{GiaoDichId=handed.GiaoDichId,PhienBanThoaThuanId=handed.PhuongAnHienTai!.Id,NguoiXacNhanId=aid,NguoiDoiTacId=delivered.NguoiNhanId,
                SanPhamId=delivered.SanPham.SanPhamId,Loai=LoaiXacNhan.DaGiao,NgayXacNhan=DateTimeOffset.UtcNow};
            await Change(db=>{db.Add(receipt);return Task.CompletedTask;});
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{handed.Id}/proposals",new TaoPhuongAnSmartSwapRequest(Guid.NewGuid(),handed.PhuongAnHienTai.Id,Plan(handed.PhuongAnHienTai,aid,1_500_000),"Không được đổi tiền sau khi đã giao sản phẩm.")),409,"No counter after any handover evidence exists");
            await Status(await a.PostAsJsonAsync($"/api/smart-swaps/{handed.Id}/cancel",new HuyVongSmartSwapRequest(handed.PhuongAnHienTai.Id,"Không hủy sau khi sản phẩm đã bàn giao.")),409,"No cancel after handover");
            await Change(async db=>db.Remove(await db.XacNhanGiaoNhan.SingleAsync(x=>x.Id==receipt.Id)));await Cancel(handed);
            var legacy=await Invite();await Change(async db=>{var row=await db.VongSmartSwap.SingleAsync(x=>x.Id==legacy.Id);row.NguoiKhoiTaoId=null;row.NgayTao=DateTimeOffset.UtcNow.AddDays(-2);row.NgayHetHan=DateTimeOffset.UtcNow.AddDays(-1);});await Worker();
            var legacyRead=await Get(legacy.Id);Check(legacyRead.TrangThai=="ChoDongY"&&!legacyRead.CoThePhanHoi&&!legacyRead.CoTheDeXuat,"Legacy circle without new creator metadata is read-only and not expired by worker");
            await Change(async db=>{var row=await db.VongSmartSwap.SingleAsync(x=>x.Id==legacy.Id);row.NguoiKhoiTaoId=aid;row.NgayHetHan=DateTimeOffset.UtcNow.AddHours(1);});await Cancel(legacy);

            var b2=await Publish(b,"Laptop second","iPad",12_000_000,0);var c2=await Publish(c,"iPad second","iPhone",8_000_000);
            var first=await Invite();var second=await Invite(ta,b2,c2);first=await Vote(first,b);second=await Vote(second,b);
            var competing=await Task.WhenAll(c.PostAsJsonAsync($"/api/smart-swaps/{first.Id}/responses",new PhanHoiSmartSwapRequest(first.PhuongAnHienTai!.Id,true,true)),c.PostAsJsonAsync($"/api/smart-swaps/{second.Id}/responses",new PhanHoiSmartSwapRequest(second.PhuongAnHienTai!.Id,true,true)));
            Check(competing.Count(x=>(int)x.StatusCode==200)==1&&competing.Count(x=>(int)x.StatusCode==409)==1,"Concurrent final votes for overlapping circles yield one success and one409");
            var winner=(int)competing[0].StatusCode==200?first:second;var loser=winner.Id==first.Id?second:first;
            Check(await Holds(winner.GiaoDichId)==3&&await Holds(loser.GiaoDichId)==0,"Losing circle acquires no partial inventory holds");
            await Cancel(winner);loser=await Vote(loser,c);Check(await Holds(loser.GiaoDichId)==3,"Failed competing acceptance can retry after stock release");await Cancel(loser);

            // Cross-workflow race: same shared product, atomic Smart Swap vs an actual order.
            await Change(async db=>{var t=await db.TinDang.SingleAsync(x=>x.Id==ta.Id);t.LoaiTin=LoaiTinDang.CaHai;t.GiaBan=10_000_000;});
            var saleCircle=await Vote(await Invite(),b);var live=(await other.GetFromJsonAsync<TinDangDto>($"/api/tindang/{ta.Id}"))!;
            var orderBody=new TaoDonRequest(Guid.NewGuid(),live.Id,live.PhienBan,null,"Người nhận demo","0901234567","123 Đường kiểm thử, Hà Nội","MoPhong");
            var cross=await Task.WhenAll(c.PostAsJsonAsync($"/api/smart-swaps/{saleCircle.Id}/responses",new PhanHoiSmartSwapRequest(saleCircle.PhuongAnHienTai!.Id,true,true)),other.PostAsJsonAsync("/api/donhang",orderBody));
            var smartWon=(int)cross[0].StatusCode==200;Check(smartWon?(int)cross[1].StatusCode==409:(int)cross[0].StatusCode==409&&(int)cross[1].StatusCode==201,"Shared stock locks serialize official Smart Swap against order creation");
            if(!smartWon){var order=(await cross[1].Content.ReadFromJsonAsync<DonHangDto>())!;await Post<DonHangDto>(other,$"/api/donhang/{order.Id}/huy",new ChuyenDonRequest(order.PhienBan,"Hủy đơn kiểm thử để giải phóng tồn kho."));}
            await Cancel(saleCircle);

            // Independent exchange1-to-1 uses the same product lock order and unique hold index.
            var td=await Publish(other,"Món trao đổi dự phòng","iPhone",10_000_000);
            var oneCircle=await Vote(await Invite(),b);var currentA=(await a.GetFromJsonAsync<TinDangDto>($"/api/tindang/{ta.Id}"))!;
            var one=await Post<DeNghiTraoDoiDto>(other,"/api/traodoi",new TaoDeNghiRequest(Guid.NewGuid(),td.Id,ta.Id,td.PhienBan,currentA.PhienBan,0,did,"Muốn đề nghị trao đổi trực tiếp không bù.",null,null,true),201);
            var oneRace=await Task.WhenAll(c.PostAsJsonAsync($"/api/smart-swaps/{oneCircle.Id}/responses",new PhanHoiSmartSwapRequest(oneCircle.PhuongAnHienTai!.Id,true,true)),a.PostAsJsonAsync($"/api/traodoi/{one.Id}/phanhoi",new PhanHoiDeNghiRequest(one.PhienBan,true)));
            var smartWin=(int)oneRace[0].StatusCode==200;Check(smartWin?(int)oneRace[1].StatusCode==409:(int)oneRace[0].StatusCode==409&&(int)oneRace[1].StatusCode==200,"Smart Swap and1-to-1 cannot reserve the same product twice");
            var oneLatest=(await other.GetFromJsonAsync<DeNghiTraoDoiDto>($"/api/traodoi/{one.Id}"))!;
            await Post<DeNghiTraoDoiDto>(other,$"/api/traodoi/{one.Id}/huy",new HuyDeNghiRequest(oneLatest.PhienBan,"Hủy đề nghị trao đổi của kịch bản kiểm thử."));await Cancel(oneCircle);
            Check(!await Db(db=>db.GiuChoSanPham.Where(x=>!x.DaGiaiPhong).GroupBy(x=>x.SanPhamId).AnyAsync(g=>g.Count()>1)),"Database has at most one live hold per product across workflows");
            Console.WriteLine($"Smart Swap workflow HTTP/SQL checks: {passed} passed. Three real participant accounts, versioned consensus and concurrent stock checks.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine("FAIL Smart Swap workflow: "+ex);return 1;}
        finally
        {
            if(factory!=null)await factory.DisposeAsync();
            if(created&&name.StartsWith("ReMarketTest_",StringComparison.Ordinal))await new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]",master).ExecuteNonQueryAsync();
            if(Directory.Exists(folder)&&folder.StartsWith(tempRoot,StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(folder)=="remarket-smart-workflow-"+name)Directory.Delete(folder,true);
        }
    }
}
