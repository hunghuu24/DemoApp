using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReMarket.Api.Controllers;
using ReMarket.Application.SanPham;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;
using SkiaSharp;

namespace ReMarket.DatabaseChecks;

internal static class KiemTraTinDang
{
    private sealed class Factory(string connection, string folder) : WebApplicationFactory<TinDangController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder
            .UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi().ConfigureLogging(x => x.ClearProviders())
            .ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?> {
                ["ConnectionStrings:ReMarket"] = connection, ["Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ["Media:ProductFolder"] = folder, ["RateLimits:PermitLimit"] = "1000"
            }));
    }
    public static async Task<int> ChayAsync()
    {
        var name = "ReMarketTest_" + Guid.NewGuid().ToString("N");
        var folder = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "remarket-products-" + name));
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket") ?? "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True") { InitialCatalog = "master" };
        await using var master = new SqlConnection(connection.ConnectionString);
        await master.OpenAsync();
        var passed = 0;
        Factory? factory = null;
        var created = false;
        void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); passed++; Console.WriteLine("PASS " + message); }
        async Task Status(HttpResponseMessage response, HttpStatusCode expected, string message)
        {
            if (response.StatusCode != expected && (int)response.StatusCode >= 400)
                Console.Error.WriteLine("HTTP error: " + await response.Content.ReadAsStringAsync());
            Check(response.StatusCode == expected, message + " [" + (int)response.StatusCode + "]");
        }
        try
        {
            await new SqlCommand($"CREATE DATABASE [{name}]", master).ExecuteNonQueryAsync(); created = true;
            connection.InitialCatalog = name;
            factory = new Factory(connection.ConnectionString, folder);
            using var anon = factory.CreateClient();
            using var owner = factory.CreateClient(); using var other = factory.CreateClient(); using var admin = factory.CreateClient();
            async Task<T> Db<T>(Func<ReMarketDbContext, Task<T>> action)
            { await using var scope = factory.Services.CreateAsyncScope(); return await action(scope.ServiceProvider.GetRequiredService<ReMarketDbContext>()); }
            await Db(async db => { await db.Database.MigrateAsync(); return true; });
            async Task<Guid> Register(HttpClient client, string prefix, string phone)
            {
                var email = prefix + name + "@example.test";
                var response = await client.PostAsJsonAsync("/api/taikhoan/dangky", new DangKyRequest(prefix, email, phone, "ListingTest!2026", "ListingTest!2026", true));
                await Status(response, HttpStatusCode.Created, "Register real " + prefix);
                var profile = (await response.Content.ReadFromJsonAsync<HoSoDto>())!;
                var login = await client.PostAsJsonAsync("/api/taikhoan/dangnhap", new DangNhapRequest(email, "ListingTest!2026"));
                await Status(login, HttpStatusCode.OK, "Login real " + prefix);
                client.DefaultRequestHeaders.Authorization = new("Bearer", (await login.Content.ReadFromJsonAsync<DangNhapDto>())!.AccessToken);
                return profile.Id;
            }
            var ownerId = await Register(owner, "Product owner", "0911234567");
            await Register(other, "Other owner", "0921234567");
            var adminId = await Register(admin, "Moderator", "0931234567");
            var root = new DanhMuc { Ten = "Nội thất", Ma = "test-noi-that", DangHienThi = true };
            var child = new DanhMuc { Ten = "Ghế", Ma = "test-ghe", DangHienThi = true, DanhMucChaId = root.Id };
            var hidden = new DanhMuc { Ten = "Ẩn", Ma = "test-an", DangHienThi = false };
            var hiddenChild = new DanhMuc { Ten = "Con ẩn", Ma = "test-con-an", DangHienThi = true, DanhMucChaId = hidden.Id };
            await Db(async db => { db.AddRange(root, child, hidden, hiddenChild); db.Add(new NguoiDungVaiTro { NguoiDungId = adminId, VaiTroId = await db.VaiTro.Where(x => x.Ma == "ADMIN").Select(x => x.Id).SingleAsync() }); await db.SaveChangesAsync(); return true; });
            var categories = await anon.GetFromJsonAsync<List<DanhMucDto>>("/api/tindang/danhmuc");
            Check(categories!.Count == 2 && categories.All(x => x.Id != hiddenChild.Id), "Hidden category ancestors excluded");
            var form = new LuuTinDangRequest("Ghế gỗ kiểm tra", child.Id, 450000, "NhuMoi", "Ghế gỗ thật, còn chắc chắn và dùng tốt.", "TP. Hồ Chí Minh", "Quận 1, TP. Hồ Chí Minh", null);
            await Status(await anon.PostAsJsonAsync("/api/tindang", form), HttpStatusCode.Unauthorized, "Anonymous cannot create");
            await Status(await owner.PostAsJsonAsync("/api/tindang", form with { GiaBan = -1 }), HttpStatusCode.BadRequest, "Negative price validation");
            await Status(await owner.PostAsJsonAsync("/api/tindang", form with { MoTa = "short" }), HttpStatusCode.BadRequest, "Short description validation");
            await Status(await owner.PostAsJsonAsync("/api/tindang", form with { TinhTrang = "999" }), HttpStatusCode.BadRequest, "Unknown condition rejected");
            await Status(await owner.PostAsJsonAsync("/api/tindang", form with { DanhMucId = hiddenChild.Id }), HttpStatusCode.BadRequest, "Cannot post under hidden ancestor");
            var response = await owner.PostAsJsonAsync("/api/tindang", form);
            await Status(response, HttpStatusCode.Created, "Create product and draft atomically");
            var tin = (await response.Content.ReadFromJsonAsync<TinDangDto>())!;
            var listingId = tin.Id;
            string Url(string suffix = "") => "/api/tindang/" + listingId + suffix;
            Check(tin.TrangThai == "BanNhap" && tin.NguoiDangId == ownerId && tin.Anh.Count == 0, "Draft owner and state from server");
            await Status(await anon.GetAsync(Url()), HttpStatusCode.NotFound, "Private draft excluded from public detail");
            await Status(await other.GetAsync(Url("/quanly")), HttpStatusCode.NotFound, "Other user cannot preview/manage draft");
            await Status(await other.PutAsJsonAsync(Url(), form with { PhienBan = tin.PhienBan }), HttpStatusCode.NotFound, "Other user cannot edit product");
            await Status(await owner.PostAsJsonAsync(Url("/gui-duyet"), new PhienBanTinRequest(tin.PhienBan)), HttpStatusCode.BadRequest, "Cannot submit without cover image");
            async Task<HttpResponseMessage> Upload(HttpClient client, string version, byte[] bytes)
            {
                using var content = new MultipartFormDataContent(); content.Add(new StringContent(version), "phienBan");
                var part = new ByteArrayContent(bytes); part.Headers.ContentType = new("image/png");
                content.Add(part, "anh", "../../untrusted.png"); return await client.PostAsync(Url("/anh"), content);
            }
            using var bitmap = new SKBitmap(320, 160); using (var canvas = new SKCanvas(bitmap)) canvas.Clear(SKColors.ForestGreen);
            using var image = SKImage.FromBitmap(bitmap); using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
            var png = encoded.ToArray();
            await Status(await Upload(owner, tin.PhienBan, Encoding.UTF8.GetBytes("<script>bad</script>")), HttpStatusCode.BadRequest, "Content decoding rejects forged image MIME");
            await Status(await Upload(other, tin.PhienBan, png), HttpStatusCode.NotFound, "Other owner cannot upload image");
            var upload = await Upload(owner, tin.PhienBan, png); await Status(upload, HttpStatusCode.OK, "Upload image through real multipart API");
            tin = (await upload.Content.ReadFromJsonAsync<TinDangDto>())!;
            var firstPhoto = tin.Anh.Single(); Check(firstPhoto.LaAnhBia, "First upload becomes cover");
            await Status(await anon.GetAsync(firstPhoto.Url), HttpStatusCode.NotFound, "Draft image private to owner/admin");
            using (var badMedia = new HttpRequestMessage(HttpMethod.Get, firstPhoto.Url))
            {
                badMedia.Headers.Authorization = new("Bearer", "invalid-image-token");
                await Status(await anon.SendAsync(badMedia), HttpStatusCode.Unauthorized, "Invalid bearer on image challenges for client refresh");
            }
            var imageResponse = await owner.GetAsync(firstPhoto.Url); await Status(imageResponse, HttpStatusCode.OK, "Owner reads private image");
            using (var sanitized = SKBitmap.Decode(await imageResponse.Content.ReadAsByteArrayAsync()))
                Check(sanitized.Width == 320 && sanitized.Height == 160 && imageResponse.Content.Headers.ContentType!.MediaType == "image/jpeg", "Image re-encoded preserving aspect ratio");
            Check(Directory.GetFiles(folder).All(f => Path.GetExtension(f) == ".jpg" && Guid.TryParseExact(Path.GetFileNameWithoutExtension(f), "N", out _)), "Generated filenames cannot traverse paths");
            var stale = tin.PhienBan;
            var second = await Upload(owner, tin.PhienBan, png); await Status(second, HttpStatusCode.OK, "Second product image"); tin = (await second.Content.ReadFromJsonAsync<TinDangDto>())!;
            await Status(await Upload(owner, stale, png), HttpStatusCode.Conflict, "Stale upload version rejected");
            Check(Directory.GetFiles(folder).Length == 2, "Rejected uploads leave no orphan file");
            var cover = await owner.PutAsJsonAsync(Url($"/anh/{tin.Anh.Last().Id}/bia"), new PhienBanTinRequest(tin.PhienBan));
            await Status(cover, HttpStatusCode.OK, "Switch cover under unique SQL index"); tin = (await cover.Content.ReadFromJsonAsync<TinDangDto>())!;
            Check(tin.Anh.Count(x => x.LaAnhBia) == 1 && tin.Anh.Last().LaAnhBia, "Exactly one cover");
            for (var i = 2; i < 8; i++) { var add = await Upload(owner, tin.PhienBan, png); await Status(add, HttpStatusCode.OK, "Upload within eight-image limit"); tin = (await add.Content.ReadFromJsonAsync<TinDangDto>())!; }
            await Status(await Upload(owner, tin.PhienBan, png), HttpStatusCode.BadRequest, "Ninth image rejected");
            var coverPhoto = tin.Anh.Single(x => x.LaAnhBia);
            var remove = await owner.SendAsync(new(HttpMethod.Delete, Url($"/anh/{coverPhoto.Id}")) { Content = JsonContent.Create(new PhienBanTinRequest(tin.PhienBan)) });
            await Status(remove, HttpStatusCode.OK, "Delete cover image"); tin = (await remove.Content.ReadFromJsonAsync<TinDangDto>())!;
            Check(tin.Anh.Count == 7 && tin.Anh.Count(x => x.LaAnhBia) == 1 && Directory.GetFiles(folder).Length == 7, "Cover reassigned and physical file removed");
            var edits = await Task.WhenAll(owner.PutAsJsonAsync(Url(), form with { Ten = "Ghế gỗ đã sửa", PhienBan = tin.PhienBan }), owner.PutAsJsonAsync(Url(), form with { Ten = "Ghế gỗ đồng thời", PhienBan = tin.PhienBan }));
            Check(edits.Count(x => x.StatusCode == HttpStatusCode.OK) == 1 && edits.Count(x => x.StatusCode == HttpStatusCode.Conflict) == 1, "Concurrent form writes cannot overwrite each other");
            tin = (await owner.GetFromJsonAsync<TinDangDto>(Url("/quanly")))!;
            var submit = await owner.PostAsJsonAsync(Url("/gui-duyet"), new PhienBanTinRequest(tin.PhienBan)); await Status(submit, HttpStatusCode.OK, "Submit draft for moderation"); tin = (await submit.Content.ReadFromJsonAsync<TinDangDto>())!;
            Check(tin.TrangThai == "ChoDuyet", "Submitted listing waits for review");
            Check((await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang"))!.TongSo == 0, "Pending listing excluded from search");
            await Status(await owner.PostAsJsonAsync($"/api/admin/tindang/{tin.Id}/duyet", new DuyetTinRequest(tin.PhienBan, true, null)), HttpStatusCode.Forbidden, "USER cannot approve own listing");
            var queue = await admin.GetFromJsonAsync<TrangTinDto>("/api/admin/tindang"); Check(queue!.Items.Single().Id == tin.Id, "ADMIN moderation queue uses SQL");
            var reject = await admin.PostAsJsonAsync($"/api/admin/tindang/{tin.Id}/duyet", new DuyetTinRequest(tin.PhienBan, false, "Cần bổ sung thông tin"));
            await Status(reject, HttpStatusCode.OK, "ADMIN rejects with reason"); tin = (await reject.Content.ReadFromJsonAsync<TinDangDto>())!;
            Check(tin.TrangThai == "TuChoi" && tin.LyDoTuChoi != null, "Owner receives rejection reason");
            tin = (await (await owner.PostAsJsonAsync(Url("/gui-duyet"), new PhienBanTinRequest(tin.PhienBan))).Content.ReadFromJsonAsync<TinDangDto>())!;
            async Task<TinDangDto> Approve(TinDangDto current)
            {
                var result = await admin.PostAsJsonAsync($"/api/admin/tindang/{current.Id}/duyet", new DuyetTinRequest(current.PhienBan, true, null));
                await Status(result, HttpStatusCode.OK, "ADMIN approves through API"); return (await result.Content.ReadFromJsonAsync<TinDangDto>())!;
            }
            tin = await Approve(tin);
            await Status(await anon.GetAsync(Url()), HttpStatusCode.OK, "Approved detail public");
            await Status(await anon.GetAsync(tin.Anh.First().Url), HttpStatusCode.OK, "Approved image publicly readable");
            var search = await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?tuKhoa=ghe%20go&giaTu=400000&giaDen=500000&tinhTrang=NhuMoi&sapXep=giatang&danhMucId=" + root.Id);
            Check(search!.TongSo == 1 && search.Items.Single().Id == listingId, "Search ignores accents and combines price/category-tree/condition filters");
            Check((await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?tuKhoa=unknown-value"))!.Items.Count == 0, "Empty search returns a real empty page");
            Check((await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?giaDen=100"))!.TongSo == 0, "Price filter changes SQL results");
            Check((await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?tinhThanh=Ha%20Noi"))!.TongSo == 0, "Location filter changes SQL results");
            await Status(await anon.GetAsync("/api/tindang?trang=0"), HttpStatusCode.BadRequest, "Page zero rejected");
            await Status(await anon.GetAsync("/api/tindang?kichThuocTrang=1000"), HttpStatusCode.BadRequest, "Page size bounded");
            await Status(await anon.GetAsync("/api/tindang?giaTu=1000&giaDen=1"), HttpStatusCode.BadRequest, "Reversed price interval rejected");
            await Status(await anon.GetAsync("/api/tindang?sapXep=unsafe"), HttpStatusCode.BadRequest, "Unknown sort rejected");
            var home = await anon.GetFromJsonAsync<TrangChuSanPhamDto>("/api/tindang/trangchu"); Check(home!.NoiBat.Single().Id == listingId && home.MoiDang.Single().Id == listingId, "Home sections use approved SQL records");
            var mine = await owner.GetFromJsonAsync<TinCuaToiDto>("/api/tindang/cuatoi?trangThai=DangHienThi"); Check(mine!.DanhSach.TongSo == 1 && mine.ThongKe.Single().SoTin == 1, "My listing filters and badges computed from SQL");
            Check((await other.GetFromJsonAsync<TinCuaToiDto>("/api/tindang/cuatoi"))!.DanhSach.TongSo == 0, "My listings never include another owner");
            var editLive = await owner.PutAsJsonAsync(Url(), form with { Ten = "Ghế gỗ sửa sau duyệt", PhienBan = tin.PhienBan }); await Status(editLive, HttpStatusCode.OK, "Edit a published listing"); tin = (await editLive.Content.ReadFromJsonAsync<TinDangDto>())!;
            Check(tin.TrangThai == "ChoDuyet", "Content edits require fresh moderation");
            await Status(await anon.GetAsync(Url()), HttpStatusCode.NotFound, "Edited listing withdrawn until review");
            tin = await Approve(tin);
            var hide = await owner.PostAsJsonAsync(Url("/an"), new PhienBanTinRequest(tin.PhienBan)); await Status(hide, HttpStatusCode.OK, "Owner hides listing"); tin = (await hide.Content.ReadFromJsonAsync<TinDangDto>())!;
            await Status(await anon.GetAsync(tin.Anh.First().Url), HttpStatusCode.NotFound, "Hidden listing images no longer public");
            tin = (await (await owner.PostAsJsonAsync(Url("/gui-duyet"), new PhienBanTinRequest(tin.PhienBan))).Content.ReadFromJsonAsync<TinDangDto>())!;
            tin = await Approve(tin);
            await Db(async db => { var transaction = new GiaoDich { Ma = "TEST-HOLD", Loai = LoaiGiaoDich.MuaBan, TrangThai = TrangThaiGiaoDich.KhoiTao }; db.Add(transaction); db.Add(new GiuChoSanPham { SanPhamId = tin.SanPhamId, GiaoDichId = transaction.Id, NgayHetHan = DateTimeOffset.UtcNow.AddHours(1) }); await db.SaveChangesAsync(); return true; });
            await Status(await owner.PostAsJsonAsync(Url("/da-ban"), new PhienBanTinRequest(tin.PhienBan)), HttpStatusCode.Conflict, "Cannot sell a reserved item outside its transaction");
            await Status(await owner.PutAsJsonAsync(Url(), form with { PhienBan = tin.PhienBan }), HttpStatusCode.Conflict, "Cannot overwrite reserved product");
            await Db(async db => { await db.GiuChoSanPham.Where(x => x.SanPhamId == tin.SanPhamId).ExecuteUpdateAsync(x => x.SetProperty(p => p.DaGiaiPhong, true)); return true; });
            var sold = await owner.PostAsJsonAsync(Url("/da-ban"), new PhienBanTinRequest(tin.PhienBan)); await Status(sold, HttpStatusCode.OK, "Confirm sold persists to SQL"); tin = (await sold.Content.ReadFromJsonAsync<TinDangDto>())!;
            Check(tin.TrangThai == "DaBan", "Sold state returned");
            await Status(await anon.GetAsync(Url()), HttpStatusCode.NotFound, "Sold listing excluded from buyers");
            await Status(await owner.PutAsJsonAsync(Url(), form with { PhienBan = tin.PhienBan }), HttpStatusCode.Conflict, "Sold product immutable");
            await Status(await other.SendAsync(new(HttpMethod.Delete, Url()) { Content = JsonContent.Create(new PhienBanTinRequest(tin.PhienBan)) }), HttpStatusCode.NotFound, "Other user cannot delete");
            await Status(await owner.SendAsync(new(HttpMethod.Delete, Url()) { Content = JsonContent.Create(new PhienBanTinRequest(tin.PhienBan)) }), HttpStatusCode.NoContent, "Owner soft deletes sold listing");
            await Status(await owner.GetAsync(Url("/quanly")), HttpStatusCode.NotFound, "Deleted listing unavailable to management");
            Check(await Db(db => db.TinDang.AnyAsync(t => t.Id == listingId && t.NgayXoa != null)), "Soft delete preserves relational history");
            Check((await owner.GetFromJsonAsync<TinCuaToiDto>("/api/tindang/cuatoi"))!.DanhSach.TongSo == 0, "Deleted listing excluded from my tabs");
            await Status(await owner.GetAsync(tin.Anh.First().Url), HttpStatusCode.NotFound, "Even owner cannot read soft-deleted images");
            var published = new List<TinDangDto>();
            for (var i = 1; i <= 3; i++)
            {
                var draft = (await (await owner.PostAsJsonAsync("/api/tindang", form with { Ten = "Bàn phân trang " + i, GiaBan = i * 100000 })).Content.ReadFromJsonAsync<TinDangDto>())!;
                listingId = draft.Id;
                draft = (await (await Upload(owner, draft.PhienBan, png)).Content.ReadFromJsonAsync<TinDangDto>())!;
                draft = (await (await owner.PostAsJsonAsync(Url("/gui-duyet"), new PhienBanTinRequest(draft.PhienBan))).Content.ReadFromJsonAsync<TinDangDto>())!;
                published.Add(await Approve(draft));
            }
            var pageOne = (await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?kichThuocTrang=2&trang=1&sapXep=giatang"))!;
            var pageTwo = (await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?kichThuocTrang=2&trang=2&sapXep=giatang"))!;
            Check(pageOne.TongSo == 3 && pageOne.TongTrang == 2 && pageOne.Items.Count == 2 && pageTwo.Items.Count == 1, "Real pagination totals across multiple SQL rows");
            Check(!pageOne.Items.Select(x => x.Id).Intersect(pageTwo.Items.Select(x => x.Id)).Any(), "Stable pages do not overlap");
            Check(pageOne.Items.Select(x => x.GiaBan).SequenceEqual(new long?[] { 100000, 200000 }) && pageTwo.Items[0].GiaBan == 300000, "Ascending price sorting applies before SQL paging");
            Check((await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?sapXep=giagiam"))!.Items.Select(x => x.GiaBan).SequenceEqual(new long?[] { 300000, 200000, 100000 }), "Descending price sort");
            Check((await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang?trang=9&kichThuocTrang=2"))!.Items.Count == 0, "Out-of-range page safely empty");
            categories = await anon.GetFromJsonAsync<List<DanhMucDto>>("/api/tindang/danhmuc");
            Check(categories!.First(x => x.Id == root.Id).SoTin == 3 && categories!.First(x => x.Id == child.Id).SoTin == 3, "Category counts include descendants and omit deleted listings");
            await Db(async db => { await db.DanhMuc.Where(x => x.Id == root.Id).ExecuteUpdateAsync(x => x.SetProperty(p => p.DangHienThi, false)); return true; });
            Check((await anon.GetFromJsonAsync<TrangTinDto>("/api/tindang"))!.TongSo == 0, "Hiding category parent withdraws descendant products");
            await Status(await anon.GetAsync("/api/tindang/" + published[0].Id), HttpStatusCode.NotFound, "Hidden category also protects detail route");
            await Db(async db => { await db.DanhMuc.Where(x => x.Id == root.Id).ExecuteUpdateAsync(x => x.SetProperty(p => p.DangHienThi, true)); return true; });
            var self = (await (await admin.PostAsJsonAsync("/api/tindang", form with { Ten = "Tin của quản trị viên" })).Content.ReadFromJsonAsync<TinDangDto>())!;
            listingId = self.Id;
            self = (await (await Upload(admin, self.PhienBan, png)).Content.ReadFromJsonAsync<TinDangDto>())!;
            self = (await (await admin.PostAsJsonAsync(Url("/gui-duyet"), new PhienBanTinRequest(self.PhienBan))).Content.ReadFromJsonAsync<TinDangDto>())!;
            await Status(await admin.PostAsJsonAsync($"/api/admin/tindang/{self.Id}/duyet", new DuyetTinRequest(self.PhienBan, true, null)), HttpStatusCode.Forbidden, "ADMIN cannot self-approve its own product");
            Console.WriteLine($"Listing HTTP/SQL checks: {passed} passed. No credentials or tokens printed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("Listing checks failed: " + ex); return 1; }
        finally
        {
            if (factory != null) await factory.DisposeAsync();
            if (created && name.StartsWith("ReMarketTest_", StringComparison.Ordinal))
                await new SqlCommand($"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]", master).ExecuteNonQueryAsync();
            var tempRoot = Path.GetFullPath(Path.GetTempPath());
            if (Directory.Exists(folder) && folder.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase) && Path.GetFileName(folder) == "remarket-products-" + name) Directory.Delete(folder, true);
        }
    }
}
