using System.Net;
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
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using SkiaSharp;

namespace ReMarket.DatabaseChecks;

internal static class KiemTraMuaBan
{
    private sealed class Factory(string connection, string folder) : WebApplicationFactory<DonHangController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder
            .UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","src","ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi().ConfigureLogging(x => x.ClearProviders())
            .ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string,string?> {
                ["ConnectionStrings:ReMarket"] = connection, ["Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ["Media:ProductFolder"] = folder, ["RateLimits:PermitLimit"] = "5000", ["Commerce:EnableExpirationWorker"] = "false"
            }));
    }
    public static async Task<int> ChayAsync()
    {
        var name = "ReMarketTest_" + Guid.NewGuid().ToString("N");
        var folder = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "remarket-commerce-" + name));
        var cs = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket") ?? "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True") { InitialCatalog = "master" };
        await using var master = new SqlConnection(cs.ConnectionString); await master.OpenAsync();
        Factory? factory = null; var created = false; var passed = 0;
        void Check(bool ok, string text) { if (!ok) throw new InvalidOperationException(text); passed++; Console.WriteLine("PASS " + text); }
        async Task Status(HttpResponseMessage r, int expected, string text)
        {
            if ((int)r.StatusCode != expected && (int)r.StatusCode >= 400) Console.Error.WriteLine(await r.Content.ReadAsStringAsync());
            Check((int)r.StatusCode == expected, text + " [" + (int)r.StatusCode + "]");
        }
        try
        {
            await new SqlCommand($"CREATE DATABASE [{name}]", master).ExecuteNonQueryAsync(); created = true;
            cs.InitialCatalog = name; factory = new Factory(cs.ConnectionString, folder);
            using var buyer = factory.CreateClient(); using var seller = factory.CreateClient(); using var other = factory.CreateClient(); using var admin = factory.CreateClient(); using var anon = factory.CreateClient();
            async Task<T> Db<T>(Func<ReMarket.Infrastructure.Persistence.ReMarketDbContext,Task<T>> action)
            { await using var scope = factory.Services.CreateAsyncScope(); return await action(scope.ServiceProvider.GetRequiredService<ReMarket.Infrastructure.Persistence.ReMarketDbContext>()); }
            await Db(async db => { await db.Database.MigrateAsync(); return true; });
            async Task<Guid> Account(HttpClient c, string label, string phone)
            {
                var email = label + name + "@example.test";
                var r = await c.PostAsJsonAsync("/api/taikhoan/dangky", new DangKyRequest(label, email, phone, "CommerceTest!2026", "CommerceTest!2026", true));
                await Status(r, 201, "Real registration " + label); var user = (await r.Content.ReadFromJsonAsync<HoSoDto>())!;
                var login = await c.PostAsJsonAsync("/api/taikhoan/dangnhap", new DangNhapRequest(email,"CommerceTest!2026")); await Status(login,200,"Real login " + label);
                c.DefaultRequestHeaders.Authorization = new("Bearer", (await login.Content.ReadFromJsonAsync<DangNhapDto>())!.AccessToken); return user.Id;
            }
            var buyerId = await Account(buyer,"Buyer","0911111111"); var sellerId = await Account(seller,"Seller","0922222222");
            var otherId = await Account(other,"Stranger","0933333333"); var adminId = await Account(admin,"Moderator","0944444444");
            var category = new DanhMuc { Ten = "Điện tử", Ma = "commerce-test", DangHienThi = true };
            await Db(async db => { db.Add(category); db.Add(new NguoiDungVaiTro { NguoiDungId = adminId, VaiTroId = await db.VaiTro.Where(x => x.Ma == "ADMIN").Select(x => x.Id).SingleAsync() }); await db.SaveChangesAsync(); return true; });
            using var bmp = new SKBitmap(240,160); using (var canvas = new SKCanvas(bmp)) canvas.Clear(SKColors.ForestGreen);
            using var skImg = SKImage.FromBitmap(bmp); using var encoded = skImg.Encode(SKEncodedImageFormat.Png,100); var png = encoded.ToArray();
            async Task<TinDangDto> Listing(string title = "Điện thoại kiểm thử")
            {
                var r = await seller.PostAsJsonAsync("/api/tindang",new LuuTinDangRequest(title,category.Id,7200000,"NhuMoi","Máy hoạt động tốt, còn dùng được và đúng mô tả.","TP. Hồ Chí Minh","Quận 1, TP. Hồ Chí Minh",null));
                await Status(r,201,"Create listing for purchase"); var tin = (await r.Content.ReadFromJsonAsync<TinDangDto>())!;
                using var body = new MultipartFormDataContent(); body.Add(new StringContent(tin.PhienBan),"phienBan"); body.Add(new ByteArrayContent(png),"anh","photo.png");
                var upload = await seller.PostAsync($"/api/tindang/{tin.Id}/anh",body); await Status(upload,200,"Upload sale picture"); tin = (await upload.Content.ReadFromJsonAsync<TinDangDto>())!;
                var submit = await seller.PostAsJsonAsync($"/api/tindang/{tin.Id}/gui-duyet",new PhienBanTinRequest(tin.PhienBan)); await Status(submit,200,"Submit sale listing"); tin = (await submit.Content.ReadFromJsonAsync<TinDangDto>())!;
                var approve = await admin.PostAsJsonAsync($"/api/admin/tindang/{tin.Id}/duyet",new DuyetTinRequest(tin.PhienBan,true,null)); await Status(approve,200,"Approve listing through real role boundary");
                return (await approve.Content.ReadFromJsonAsync<TinDangDto>())!;
            }
            async Task<T> Post<T>(HttpClient c,string url,object body,int expected,string message)
            { var r=await c.PostAsJsonAsync(url,body); await Status(r,expected,message); return (await r.Content.ReadFromJsonAsync<T>())!; }
            async Task<BaoGiaDto> Quote(HttpClient c,Guid tin,Guid? coupon=null) => await Post<BaoGiaDto>(c,"/api/donhang/baogia",new BaoGiaRequest(tin,coupon),200,"Server computes checkout quote");
            TaoDonRequest CreateBody(BaoGiaDto q,Guid? key=null) => new(key ?? Guid.NewGuid(),q.SanPham.TinDangId,q.PhienBanTin,q.PhieuThuongLuongId,"Người nhận kiểm thử","0911111111","Quận 1, TP. Hồ Chí Minh","MoPhong");
            var listing = await Listing();
            await Status(await anon.PostAsJsonAsync("/api/donhang/baogia",new BaoGiaRequest(listing.Id,null)),401,"Anonymous cannot checkout");
            await Status(await seller.PostAsJsonAsync("/api/donhang/baogia",new BaoGiaRequest(listing.Id,null)),403,"Cannot buy own product");
            var chat = await Post<HoiThoaiDto>(buyer,"/api/hoithoai",new MoChatRequest(listing.Id),200,"Buyer opens negotiation");
            await Status(await buyer.GetAsync("/api/hopthu"),200,"Legacy inbox reads only current user's conversations");
            await Status(await buyer.GetAsync($"/api/hoithoai/{chat.Id}/sanpham/anh"),200,"Participant reads actual chat product photo");
            var again = await Post<HoiThoaiDto>(buyer,"/api/hoithoai",new MoChatRequest(listing.Id),200,"Repeated open reuses conversation"); Check(chat.Id==again.Id,"Unique buyer/listing conversation");
            await Status(await other.GetAsync($"/api/hoithoai/{chat.Id}"),404,"Stranger cannot read chat");
            await Status(await other.PostAsJsonAsync($"/api/hoithoai/{chat.Id}/tinnhan",new GuiTinRequest(Guid.NewGuid(),"Không có quyền")),404,"Stranger cannot send into chat");
            var keyMsg=Guid.NewGuid(); var msg=new GuiTinRequest(keyMsg,"Chào bạn, mình muốn thương lượng.");
            chat=await Post<HoiThoaiDto>(buyer,$"/api/hoithoai/{chat.Id}/tinnhan",msg,200,"Send persisted message");
            chat=await Post<HoiThoaiDto>(buyer,$"/api/hoithoai/{chat.Id}/tinnhan",msg,200,"Retry same message");
            Check(chat.TinNhan.Count(x=>x.NoiDung==msg.NoiDung)==1,"Message idempotence prevents duplicates");
            await Status(await buyer.PostAsJsonAsync($"/api/hoithoai/{chat.Id}/tinnhan",msg with {NoiDung="Nội dung khác"}),409,"Cannot reuse message ID for new content");
            await Status(await buyer.PostAsJsonAsync($"/api/hoithoai/{chat.Id}/tinnhan",new GuiTinRequest(Guid.NewGuid(),"   ")),400,"Blank chat message rejected");
            var inbox=(await seller.GetFromJsonAsync<List<HopThuMuaDto>>("/api/hoithoai?chuaDoc=true"))!;
            Check(inbox.Single().ChuaDoc==1,"Unread count uses message sequence");
            await seller.GetAsync($"/api/hoithoai/{chat.Id}"); Check((await seller.GetFromJsonAsync<List<HopThuMuaDto>>("/api/hoithoai?chuaDoc=true"))!.Count==0,"Opening chat clears unread watermark");
            using(var body=new MultipartFormDataContent())
            {
                body.Add(new StringContent(Guid.NewGuid().ToString()),"maPhiaClient");body.Add(new ByteArrayContent(png),"anh","../../image.png");
                chat=await (await buyer.PostAsync($"/api/hoithoai/{chat.Id}/anh",body)).Content.ReadFromJsonAsync<HoiThoaiDto>() ?? throw new InvalidOperationException("Image response");
            }
            var imageUrl=chat.TinNhan.Last().Anh.Single().Url;
            await Status(await seller.GetAsync(imageUrl),200,"Participant sees sanitized chat image"); await Status(await other.GetAsync(imageUrl),404,"Nonmember cannot see chat image"); await Status(await anon.GetAsync(imageUrl),401,"Chat image needs authentication");
            await Status(await buyer.PostAsJsonAsync($"/api/hoithoai/{chat.Id}/phieu",new TaoPhieuRequest(Guid.NewGuid(),7200000)),400,"Discount cannot consume entire product price");
            await Status(await seller.PostAsJsonAsync($"/api/hoithoai/{chat.Id}/phieu",new TaoPhieuRequest(Guid.NewGuid(),700000)),403,"Seller cannot impersonate buyer's proposal");
            chat=await Post<HoiThoaiDto>(buyer,$"/api/hoithoai/{chat.Id}/phieu",new TaoPhieuRequest(Guid.NewGuid(),700000),200,"Create negotiation voucher"); var coupon=chat.Phieu.Last();
            await Status(await buyer.PostAsJsonAsync($"/api/hoithoai/{chat.Id}/phieu/{coupon.Id}",new PhanHoiPhieuRequest(coupon.PhienBan,true)),403,"Buyer cannot approve own reduction");
            await Status(await buyer.PostAsJsonAsync("/api/donhang/baogia",new BaoGiaRequest(listing.Id,coupon.Id)),409,"Unaccepted voucher not applied");
            chat=await Post<HoiThoaiDto>(seller,$"/api/hoithoai/{chat.Id}/phieu/{coupon.Id}",new PhanHoiPhieuRequest(coupon.PhienBan,false),200,"Seller rejects offer"); Check(chat.Phieu.Single(x=>x.Id==coupon.Id).TrangThai=="TuChoi","Rejection state persisted");
            chat=await Post<HoiThoaiDto>(buyer,$"/api/hoithoai/{chat.Id}/phieu",new TaoPhieuRequest(Guid.NewGuid(),700000),200,"New proposal after rejection");coupon=chat.Phieu.Last();
            chat=await Post<HoiThoaiDto>(seller,$"/api/hoithoai/{chat.Id}/phieu/{coupon.Id}",new PhanHoiPhieuRequest(coupon.PhienBan,true),200,"Seller accepts voucher");coupon=chat.Phieu.Single(x=>x.Id==coupon.Id);
            await Status(await other.PostAsJsonAsync("/api/donhang/baogia",new BaoGiaRequest(listing.Id,coupon.Id)),400,"Voucher bound to exact buyer");
            var second=await Listing("Sản phẩm khác"); await Status(await buyer.PostAsJsonAsync("/api/donhang/baogia",new BaoGiaRequest(second.Id,coupon.Id)),400,"Voucher bound to exact product");
            var quote=await Quote(buyer,listing.Id,coupon.Id); Check(quote.GiaNiemYet==7200000 && quote.SoTienGiam==700000 && quote.TienHang==6500000 && quote.TongTien==6530000,"Totals computed server-side with accepted discount and shipping");
            var create=CreateBody(quote);
            await Status(await buyer.PostAsJsonAsync("/api/donhang",create with {PhuongThuc="Bank"}),400,"Real bank method not accepted");
            await Status(await buyer.PostAsJsonAsync("/api/donhang",create with {SoDienThoai="bad"}),400,"Recipient phone validation");
            var order=await Post<DonHangDto>(buyer,"/api/donhang",create,201,"Create reserved order atomically"); var orderId=order.Id;
            var orderChat=await Post<HoiThoaiDto>(buyer,$"/api/donhang/{order.Id}/hoithoai",new{},200,"Order opens authorized negotiation conversation");Check(orderChat.Id==chat.Id,"Order chat reuses the buyer/product conversation");
            Check(order.NguoiMuaId==buyerId && order.NguoiBanId==sellerId && order.TrangThai=="ChoThanhToan" && order.ThanhToan.LaMoPhong,"Order parties and initial simulation state");
            var replay=await Post<DonHangDto>(buyer,"/api/donhang",create,201,"Retry creation after response loss");Check(replay.Id==order.Id,"Order request key prevents duplicate order");
            await Status(await buyer.PostAsJsonAsync("/api/donhang",create with {DiaChi="Địa chỉ đã thay đổi"}),409,"Request key cannot be reused for changed checkout");
            await Status(await other.GetAsync($"/api/donhang/{order.Id}"),404,"Stranger cannot read order or address");await Status(await seller.GetAsync($"/api/donhang/{order.Id}"),200,"Seller may read own sold order");
            await Status(await anon.GetAsync($"/api/tindang/{listing.Id}"),404,"Reserved product removed from public catalog");
            await Status(await buyer.GetAsync(order.SanPham.AnhUrl!),200,"Buyer still sees own order picture after reservation");await Status(await other.GetAsync(order.SanPham.AnhUrl!),404,"Order picture ownership checked");
            await Status(await seller.PostAsJsonAsync($"/api/tindang/{listing.Id}/da-ban",new PhienBanTinRequest(listing.PhienBan)),409,"Catalog cannot manually sell reserved inventory");
            Check(await Db(db=>db.GiuChoSanPham.CountAsync(x=>x.SanPhamId==listing.SanPhamId&&!x.DaGiaiPhong))==1,"One active reservation stored in SQL");
            var saleList=(await seller.GetFromJsonAsync<TrangDonDto>("/api/donhang?vaiTro=ban"))!;Check(saleList.TongSo==1,"Seller order list uses owner filter");Check((await other.GetFromJsonAsync<TrangDonDto>("/api/donhang"))!.TongSo==0,"Other buyer sees no private orders");
            await Status(await buyer.PostAsJsonAsync($"/api/donhang/{order.Id}/nhanhang",new PhienBanTinRequest(order.PhienBan)),409,"Cannot receive before payment/delivery");
            await Status(await buyer.PostAsJsonAsync($"/api/donhang/{order.Id}/danhgia",new GuiDanhGiaRequest(Guid.NewGuid(),5,5,"Rất tốt")),409,"Cannot review incomplete transaction");
            await Status(await seller.PostAsJsonAsync($"/api/donhang/{order.Id}/thanhtoan",new ThanhToanRequest(Guid.NewGuid(),order.PhienBan,true)),403,"Seller cannot pay as buyer");
            var failure=new ThanhToanRequest(Guid.NewGuid(),order.PhienBan,false); order=await Post<DonHangDto>(buyer,$"/api/donhang/{order.Id}/thanhtoan",failure,200,"Persist simulated payment failure");
            Check(order.ThanhToan.TrangThai=="ThatBai"&&order.TrangThai=="ChoThanhToan","Failed payment keeps unpaid order for retry/cancel");
            await Post<DonHangDto>(buyer,$"/api/donhang/{order.Id}/thanhtoan",failure,200,"Payment result replay idempotent");
            await Status(await buyer.PostAsJsonAsync($"/api/donhang/{order.Id}/thanhtoan",failure with {ThanhCong=true}),409,"Payment key cannot change result");
            order=await Post<DonHangDto>(buyer,$"/api/donhang/{order.Id}/thanhtoan",new ThanhToanRequest(Guid.NewGuid(),order.PhienBan,true),200,"Successful payment retry");Check(order.ThanhToan.DangGiuTien && order.TrangThai=="DaThanhToan","Simulated held amount and paid state persisted");
            await Status(await buyer.PostAsJsonAsync($"/api/donhang/{order.Id}/vanchuyen",new ChuyenShipRequest(order.PhienBan,"DaLayHang")),403,"Buyer cannot impersonate shipping/seller");
            await Status(await seller.PostAsJsonAsync($"/api/donhang/{order.Id}/vanchuyen",new ChuyenShipRequest(order.PhienBan,"DaGiao")),409,"Shipping cannot skip handover/transit");
            foreach(var state in new[]{"DaLayHang","DangVanChuyen","GiaoThatBai","DangVanChuyen","DaGiao"})
                order=await Post<DonHangDto>(seller,$"/api/donhang/{order.Id}/vanchuyen",new ChuyenShipRequest(order.PhienBan,state),200,"Persist shipping step "+state);
            Check(order.VanChuyen!.LichSu.Select(x=>x.TrangThai).SequenceEqual(new[]{"ChoLayHang","DaLayHang","DangVanChuyen","GiaoThatBai","DangVanChuyen","DaGiao"}),"Shipping history retains exact event order, including retry");
            await Status(await seller.PostAsJsonAsync($"/api/donhang/{order.Id}/nhanhang",new PhienBanTinRequest(order.PhienBan)),403,"Seller cannot confirm buyer receipt");
            await Status(await buyer.PostAsJsonAsync($"/api/donhang/{order.Id}/huy",new ChuyenDonRequest(order.PhienBan,"Không muốn mua nữa")),409,"No cancellation after shipping handover");
            order=await Post<DonHangDto>(buyer,$"/api/donhang/{order.Id}/nhanhang",new PhienBanTinRequest(order.PhienBan),200,"Buyer confirms completion");
            Check(order.TrangThai=="DaNhan"&&order.ThanhToan.NgayGiaiNgan!=null&&!order.ThanhToan.DangGiuTien,"Receipt releases simulated held amount");
            Check(await Db(db=>db.GiaoDich.AnyAsync(x=>x.Id==db.DonHang.Where(d=>d.Id==orderId).Select(d=>d.GiaoDichId).Single()&&x.TrangThai==TrangThaiGiaoDich.HoanTat)),"Transaction completed in SQL after valid receipt");
            await Post<DonHangDto>(buyer,$"/api/donhang/{order.Id}/nhanhang",new PhienBanTinRequest(order.PhienBan),200,"Receipt retry does not duplicate confirmation");
            var rating=new GuiDanhGiaRequest(Guid.NewGuid(),5,4,"Người bán đúng hẹn, sản phẩm đúng mô tả.");
            var review=await Post<DanhGiaMuaDto>(buyer,$"/api/donhang/{order.Id}/danhgia",rating,200,"Buyer rates product and seller");Check(review.SoSaoNguoiBan==5&&review.SoSaoSanPham==4,"Separate product and seller ratings stored");
            await Post<DanhGiaMuaDto>(buyer,$"/api/donhang/{order.Id}/danhgia",rating,200,"Same review submission is idempotent");
            await Status(await buyer.PostAsJsonAsync($"/api/donhang/{order.Id}/danhgia",rating with {MaYeuCau=Guid.NewGuid()}),409,"Duplicate review rejected");
            await Status(await seller.PostAsJsonAsync($"/api/donhang/{order.Id}/danhgia",rating),403,"Seller cannot rate itself");
            var help=new TaoHoTroRequest(Guid.NewGuid(),order.Id,"DonHangVanChuyen","Cần hỗ trợ đơn hàng","Mong đội ngũ kiểm tra giúp trạng thái và thông tin của đơn.");
            await Status(await other.PostAsJsonAsync("/api/hotro",help),404,"Cannot request support for stranger's order");
            var ticket=await Post<HoTroDto>(buyer,"/api/hotro",help,201,"Create support request bound to own order");
            Check((await buyer.GetFromJsonAsync<List<HoTroDto>>("/api/hotro"))!.Any(x=>x.Id==ticket.Id),"Owner support list contains real created ticket");
            Check((await admin.GetFromJsonAsync<List<HoTroDto>>("/api/admin/hotro"))!.Any(x=>x.Id==ticket.Id),"Admin support list contains real created ticket");
            await Status(await admin.GetAsync($"/api/admin/hotro/{ticket.Id}"),200,"Admin reads actual support detail");
            var ticketRetry=await Post<HoTroDto>(buyer,"/api/hotro",help,201,"Retry support request");Check(ticket.Id==ticketRetry.Id,"Support idempotence");
            await Status(await other.GetAsync($"/api/hotro/{ticket.Id}"),404,"Support privacy enforced");
            await Status(await seller.GetAsync($"/api/hotro/{ticket.Id}"),404,"Order seller cannot read buyer's private support ticket");
            await Status(await buyer.PostAsJsonAsync($"/api/hotro/{ticket.Id}/phanhoi",new PhanHoiHoTroRequest(ticket.PhienBan,"Tự nâng trạng thái","DaGiaiQuyet")),403,"User cannot impersonate support admin");
            ticket=await Post<HoTroDto>(buyer,$"/api/hotro/{ticket.Id}/phanhoi",new PhanHoiHoTroRequest(ticket.PhienBan,"Tôi muốn bổ sung thêm thông tin.",null),200,"User adds information");
            ticket=await Post<HoTroDto>(admin,$"/api/admin/hotro/{ticket.Id}/phanhoi",new PhanHoiHoTroRequest(ticket.PhienBan,"Đội ngũ đang kiểm tra yêu cầu.","DangXuLy"),200,"Admin processes support");
            ticket=await Post<HoTroDto>(admin,$"/api/admin/hotro/{ticket.Id}/phanhoi",new PhanHoiHoTroRequest(ticket.PhienBan,"Đã xác minh và giải quyết vấn đề.","DaGiaiQuyet"),200,"Admin resolves support");
            Check(ticket.LichSu.Select(x=>x.TrangThai).SequenceEqual(new[]{"Moi","Moi","DangXuLy","DaGiaiQuyet"}),"Support history ordered even within the same second");
            await Status(await buyer.PostAsJsonAsync($"/api/hotro/{ticket.Id}/phanhoi",new PhanHoiHoTroRequest(ticket.PhienBan,"Bổ sung sau đóng",null)),409,"Closed support immutable");
            await Db(async db=>{await db.LichSuHoTro.Where(x=>x.YeuCauHoTroId==ticket.Id).ExecuteDeleteAsync();return true;});
            var legacyTicket=(await buyer.GetFromJsonAsync<HoTroDto>($"/api/hotro/{ticket.Id}"))!;
            Check(legacyTicket.LichSu.Count==4&&legacyTicket.LichSu.Any(x=>x.NoiDung=="Đã xác minh và giải quyết vấn đề."),"Legacy support replies remain visible after migration");
            Check(legacyTicket.LichSu.Skip(1).All(x=>x.TrangThai=="PhanHoi"),"Legacy replies do not fabricate historical processing states");
            var quoteA=await Quote(buyer,second.Id);var quoteB=await Quote(other,second.Id);
            var competing=await Task.WhenAll(buyer.PostAsJsonAsync("/api/donhang",CreateBody(quoteA)),other.PostAsJsonAsync("/api/donhang",CreateBody(quoteB)));
            Check(competing.Count(x=>x.StatusCode==HttpStatusCode.Created)==1&&competing.Count(x=>x.StatusCode==HttpStatusCode.Conflict)==1,"Two buyers cannot reserve the same one-off product");
            var winner=competing[0].StatusCode==HttpStatusCode.Created?buyer:other;
            var won=(await competing.First(x=>x.StatusCode==HttpStatusCode.Created).Content.ReadFromJsonAsync<DonHangDto>())!;
            won=await Post<DonHangDto>(winner,$"/api/donhang/{won.Id}/huy",new ChuyenDonRequest(won.PhienBan,"Hủy đơn chưa thanh toán"),200,"Cancel releases unpaid reservation");
            await Status(await anon.GetAsync($"/api/tindang/{second.Id}"),200,"Cancelled stock returns to public catalog");
            var next=await Post<DonHangDto>(buyer,"/api/donhang",CreateBody(await Quote(buyer,second.Id)),201,"Rebuy released product");
            next=await Post<DonHangDto>(buyer,$"/api/donhang/{next.Id}/thanhtoan",new ThanhToanRequest(Guid.NewGuid(),next.PhienBan,true),200,"Pay before cancellation");
            next=await Post<DonHangDto>(buyer,$"/api/donhang/{next.Id}/huy",new ChuyenDonRequest(next.PhienBan,"Hủy trước bàn giao vận chuyển"),200,"Cancel paid but unshipped order");
            Check(next.ThanhToan.TrangThai=="HoanTien"&&next.ThanhToan.NgayHoanTien!=null,"Simulated refund persisted without bank integration");
            var expiry=await Post<DonHangDto>(buyer,"/api/donhang",CreateBody(await Quote(buyer,second.Id)),201,"Reserve checkout for expiration check");
            await Db(async db=>{await db.DonHang.Where(x=>x.Id==expiry.Id).ExecuteUpdateAsync(x=>x.SetProperty(p=>p.HanThanhToan,DateTimeOffset.UtcNow.AddMinutes(-1)));return true;});
            expiry=(await buyer.GetFromJsonAsync<DonHangDto>($"/api/donhang/{expiry.Id}"))!;Check(expiry.TrangThai=="DaHuy","Reading expired unpaid order persists cancellation");
            Check(!await Db(db=>db.GiuChoSanPham.AnyAsync(x=>x.SanPhamId==second.SanPhamId&&!x.DaGiaiPhong)),"Expired reservation released in SQL");
            await Status(await anon.GetAsync($"/api/tindang/{second.Id}"),200,"Expired stock available again");
            Console.WriteLine($"Commerce HTTP/SQL checks: {passed} passed. No credentials or tokens printed.");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine("Commerce checks failed: "+ex);return 1;}
        finally
        {
            if(factory!=null)await factory.DisposeAsync();
            if(created)await new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]",master).ExecuteNonQueryAsync();
            if(Directory.Exists(folder)&&folder.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase)&&Path.GetFileName(folder)=="remarket-commerce-"+name)Directory.Delete(folder,true);
        }
    }
}
