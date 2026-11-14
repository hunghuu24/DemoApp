using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ReMarket.Api.Controllers;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Authentication;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.DatabaseChecks;

internal static class KiemTraTaiKhoan
{
    private sealed class ApiFactory(string connection, string folder, string key) : WebApplicationFactory<TaiKhoanController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder
            .UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi()
            .ConfigureLogging(logging => logging.ClearProviders())
            .ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> {
                ["ConnectionStrings:ReMarket"] = connection, ["Jwt:SigningKey"] = key,
                ["Smtp:Host"] = "", ["Smtp:DevelopmentOutbox"] = Path.Combine(folder, "mail"),
                ["Media:AvatarFolder"] = Path.Combine(folder, "avatars"), ["RateLimits:PermitLimit"] = "500",
                ["Logging:LogLevel:Default"] = "Critical"
            }));
    }
    public static async Task<int> ChayAsync()
    {
        var run = Guid.NewGuid().ToString("N");
        var name = "ReMarketTest_" + run;
        var folder = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "remarket-auth-" + run));
        var baseConnection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket")
            ?? "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True");
        baseConnection.InitialCatalog = "master";
        await using var master = new SqlConnection(baseConnection.ConnectionString);
        await master.OpenAsync();
        using var quoted = new SqlCommandBuilder();
        var database = quoted.QuoteIdentifier(name);
        var created = false;
        ApiFactory? factory = null;
        var passed = 0;
        void Check(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException(message);
            passed++; Console.WriteLine("PASS " + message);
        }
        async Task Status(HttpResponseMessage response, HttpStatusCode expected, string message)
        {
            _ = await response.Content.ReadAsByteArrayAsync();
            Check(response.StatusCode == expected, message + " [" + (int)response.StatusCode + "]");
        }
        try
        {
            await new SqlCommand("CREATE DATABASE " + database, master).ExecuteNonQueryAsync(); created = true;
            baseConnection.InitialCatalog = name;
            var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            factory = new ApiFactory(baseConnection.ConnectionString, folder, key);
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            async Task<T> WithDb<T>(Func<ReMarketDbContext, Task<T>> action)
            {
                await using var scope = factory.Services.CreateAsyncScope();
                return await action(scope.ServiceProvider.GetRequiredService<ReMarketDbContext>());
            }
            await WithDb(async db => { await db.Database.MigrateAsync(); return true; });
            Check(true, "Fresh SQL migrations and USER/ADMIN roles");
            foreach(var route in new[]{"/health/live","/health/ready","/api/hethong/trangthai"})await Status(await client.GetAsync(route),HttpStatusCode.OK,"Health/database readiness "+route);
            var email = "account-" + run + "@example.test";
            const string password = "AccountTest!2026";
            var register = new DangKyRequest("Kiểm tra tài khoản", email, "0981234567", password, password, true);
            await Status(await client.PostAsJsonAsync("/api/taikhoan/dangky", register with { MatKhau = "short", XacNhanMatKhau = "short" }), HttpStatusCode.BadRequest, "Weak password rejected");
            var registered = await client.PostAsJsonAsync("/api/taikhoan/dangky", register);
            await Status(registered, HttpStatusCode.Created, "Real registration creates account");
            var profile = (await registered.Content.ReadFromJsonAsync<HoSoDto>())!;
            Check(profile.VaiTro.SequenceEqual(["USER"]) && profile.HoTen == register.HoTen, "Registration only grants USER");
            var stored = await WithDb(db => db.NguoiDung.AsNoTracking().SingleAsync(x => x.Id == profile.Id));
            var hasher = new PasswordHasher<NguoiDung>();
            Check(stored.MatKhauBam != password && hasher.VerifyHashedPassword(stored, stored.MatKhauBam, password) != PasswordVerificationResult.Failed,
                "Password stored as a valid salted PBKDF2 hash");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/dangky", register with { Email = email.ToUpperInvariant() }), HttpStatusCode.Conflict, "Duplicate normalized email rejected");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/dangky", new { register.HoTen, register.Email, register.SoDienThoai, register.MatKhau, register.XacNhanMatKhau, register.DongYDieuKhoan, vaiTro = "ADMIN" }),
                HttpStatusCode.BadRequest, "Client cannot add ADMIN field");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/dangnhap", new DangNhapRequest(email, "incorrect")), HttpStatusCode.Unauthorized, "Wrong password rejected");
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Anonymous profile rejected");
            async Task<DangNhapDto> Login(string account, string pass)
            {
                var response = await client.PostAsJsonAsync("/api/taikhoan/dangnhap", new DangNhapRequest(account, pass));
                await Status(response, HttpStatusCode.OK, "Login through real password verification");
                return (await response.Content.ReadFromJsonAsync<DangNhapDto>())!;
            }
            void Bearer(string token) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var session = await Login("+84981234567", password);
            Bearer(session.AccessToken);
            await Status(await client.GetAsync("/api/trangchu"), HttpStatusCode.OK, "USER can access home");
            await Status(await client.GetAsync("/api/admin/tongquan"), HttpStatusCode.Forbidden, "USER forbidden from ADMIN endpoint");
            var me = (await client.GetFromJsonAsync<HoSoDto>("/api/taikhoan/toi"))!;
            Check(me.Id == profile.Id && me.Email == email, "Profile identity comes from JWT subject");
            var update = new CapNhatHoSoRequest("Tên hồ sơ đã sửa", email, register.SoDienThoai, "Quận 3, TP. Hồ Chí Minh", "TP. Hồ Chí Minh", me.PhienBan);
            var saved = await client.PutAsJsonAsync("/api/taikhoan/toi", update);
            await Status(saved, HttpStatusCode.OK, "Profile persists to SQL");
            Check((await saved.Content.ReadFromJsonAsync<HoSoDto>())!.HoTen == update.HoTen, "Saved profile returned");
            await Status(await client.PutAsJsonAsync("/api/taikhoan/toi", update), HttpStatusCode.Conflict, "Stale profile rowversion rejected");
            var jwtHandler = new JwtSecurityTokenHandler();
            var decoded = jwtHandler.ReadJwtToken(session.AccessToken);
            string Signed(string issuer, string audience, DateTime notBefore, DateTime expires) => jwtHandler.WriteToken(new JwtSecurityToken(issuer, audience,
                decoded.Claims.Where(x => x.Type is not ("iss" or "aud" or "exp" or "nbf" or "iat")),
                notBefore, expires, new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(key)), SecurityAlgorithms.HmacSha256)));
            Bearer(session.AccessToken[..^5] + "aaaaa");
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Tampered signature rejected");
            Bearer(Signed("WrongIssuer", "ReMarket.Android", DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(5)));
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Wrong issuer rejected");
            Bearer(Signed("ReMarket", "WrongAudience", DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(5)));
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Wrong audience rejected");
            Bearer(Signed("ReMarket", "ReMarket.Android", DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1)));
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Expired access token rejected");
            var refreshed = await client.PostAsJsonAsync("/api/taikhoan/lam-moi", new RefreshRequest(session.RefreshToken));
            await Status(refreshed, HttpStatusCode.OK, "Refresh rotates session credentials");
            var next = (await refreshed.Content.ReadFromJsonAsync<DangNhapDto>())!;
            await Status(await client.PostAsJsonAsync("/api/taikhoan/lam-moi", new RefreshRequest(session.RefreshToken)), HttpStatusCode.Unauthorized, "Old refresh token cannot replay");
            Bearer(session.AccessToken);
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Old access invalid after refresh");
            Bearer(next.AccessToken);
            using (var form = new MultipartFormDataContent())
            {
                var png = await File.ReadAllBytesAsync(Path.Combine(factory.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath, "wwwroot", "demo", "san-pham.png"));
                form.Add(new ByteArrayContent(png), "anh", "test.png");
                await Status(await client.PostAsync("/api/taikhoan/anh-dai-dien", form), HttpStatusCode.OK, "Avatar decoded, sanitized and saved");
            }
            var avatar = await client.GetAsync("/api/taikhoan/anh-dai-dien");
            Check(avatar.StatusCode == HttpStatusCode.OK && avatar.Content.Headers.ContentType?.MediaType == "image/jpeg", "Authenticated avatar served as JPEG");
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent("<svg/>"), "anh", "bad.svg");
                await Status(await client.PostAsync("/api/taikhoan/anh-dai-dien", form), HttpStatusCode.BadRequest, "Unsupported avatar format rejected");
            }
            var known = await client.PostAsJsonAsync("/api/taikhoan/quenmatkhau", new QuenMatKhauRequest(email));
            var unknown = await client.PostAsJsonAsync("/api/taikhoan/quenmatkhau", new QuenMatKhauRequest("unknown@example.test"));
            await Status(known, HttpStatusCode.Accepted, "Forgot password queues recovery message");
            Check(await known.Content.ReadAsStringAsync() == await unknown.Content.ReadAsStringAsync(), "Forgot response does not reveal account or token");
            var mailFile = Directory.GetFiles(Path.Combine(folder, "mail")).Single();
            using var mail = JsonDocument.Parse(await File.ReadAllTextAsync(mailFile));
            var link = new Uri(mail.RootElement.GetProperty("LienKet").GetString()!);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(link.Query);
            var secret = query["token"].ToString();
            Check(await WithDb(db => db.YeuCauDatLaiMatKhau.AnyAsync(x => x.NguoiDungId == profile.Id && x.BamToken == TokenService.Bam(secret))), "Only hash of recovery token stored in SQL");
            var expiredSecret = TokenService.TaoBiMat();
            await WithDb(async db => {
                db.Add(new YeuCauDatLaiMatKhau { NguoiDungId = profile.Id, BamToken = TokenService.Bam(expiredSecret),
                    NgayTao = DateTimeOffset.UtcNow.AddHours(-1), NgayHetHan = DateTimeOffset.UtcNow.AddMinutes(-1) });
                return await db.SaveChangesAsync();
            });
            var newPass = "NewAccount!2026";
            await Status(await client.PostAsJsonAsync("/api/taikhoan/datlaimatkhau", new DatLaiMatKhauRequest(email, expiredSecret, newPass, newPass)), HttpStatusCode.BadRequest, "Expired recovery token rejected");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/datlaimatkhau", new DatLaiMatKhauRequest(email, TokenService.TaoBiMat(), newPass, newPass)), HttpStatusCode.BadRequest, "Invalid recovery token rejected");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/datlaimatkhau", new DatLaiMatKhauRequest(email, secret, newPass, newPass)), HttpStatusCode.NoContent, "Recovery token resets real password");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/datlaimatkhau", new DatLaiMatKhauRequest(email, secret, newPass, newPass)), HttpStatusCode.BadRequest, "Recovery token is single use");
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Password reset revokes existing access");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/lam-moi", new RefreshRequest(next.RefreshToken)), HttpStatusCode.Unauthorized, "Password reset revokes refresh");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/dangnhap", new DangNhapRequest(email, password)), HttpStatusCode.Unauthorized, "Old password no longer works");
            session = await Login(email, newPass); Bearer(session.AccessToken);
            await Status(await client.PostAsync("/api/taikhoan/dangxuat", null), HttpStatusCode.NoContent, "Logout revokes server session");
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Logged-out access rejected");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/lam-moi", new RefreshRequest(session.RefreshToken)), HttpStatusCode.Unauthorized, "Logged-out refresh rejected");
            session = await Login(email, newPass); Bearer(session.AccessToken);
            var lastPass = "ChangedAccount!2026";
            await Status(await client.PostAsJsonAsync("/api/taikhoan/doimatkhau", new DoiMatKhauRequest("wrong", lastPass, lastPass)), HttpStatusCode.BadRequest, "Change password checks current password");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/doimatkhau", new DoiMatKhauRequest(newPass, lastPass, lastPass)), HttpStatusCode.NoContent, "Authenticated password change persists");
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Password change revokes current token");
            session = await Login(email, lastPass);
            var otherDevice = await Login(email, lastPass);
            Bearer(session.AccessToken);
            await Status(await client.PostAsync("/api/taikhoan/dangxuat", null), HttpStatusCode.NoContent, "Current device logout succeeds");
            Bearer(otherDevice.AccessToken);
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.OK, "Other device session survives current-device logout");
            // Provision an ADMIN only inside the isolated test DB, never through registration input.
            var adminEmail = "admin-" + run + "@example.test";
            var adminCreated = await client.PostAsJsonAsync("/api/taikhoan/dangky", register with { Email = adminEmail, SoDienThoai = "0981234568" });
            var admin = (await adminCreated.Content.ReadFromJsonAsync<HoSoDto>())!;
            await WithDb(async db => { var role = await db.VaiTro.SingleAsync(x => x.Ma == "ADMIN"); db.Add(new NguoiDungVaiTro { NguoiDungId = admin.Id, VaiTroId = role.Id }); return await db.SaveChangesAsync(); });
            var adminSession = await Login(adminEmail, password); Bearer(adminSession.AccessToken);
            Check(adminSession.HoSo.VaiTro.Contains("ADMIN"), "Database role emitted as ADMIN");
            await Status(await client.GetAsync("/api/admin/tongquan"), HttpStatusCode.OK, "ADMIN can access management");
            await WithDb(db => db.NguoiDungVaiTro.Where(x => x.NguoiDungId == admin.Id && x.VaiTro.Ma == "ADMIN").ExecuteDeleteAsync());
            await Status(await client.GetAsync("/api/admin/tongquan"), HttpStatusCode.Forbidden, "Removing SQL role immediately changes authorization");
            await WithDb(db => db.NguoiDung.Where(x => x.Id == admin.Id).ExecuteUpdateAsync(x => x.SetProperty(u => u.TrangThai, TrangThaiNguoiDung.DaKhoa)));
            await Status(await client.GetAsync("/api/taikhoan/toi"), HttpStatusCode.Unauthorized, "Locked account token rejected");
            await Status(await client.PostAsJsonAsync("/api/taikhoan/dangnhap", new DangNhapRequest(adminEmail, password)), HttpStatusCode.Unauthorized, "Locked account cannot log in");
            Console.WriteLine($"Account HTTP/SQL checks: {passed} passed. No credentials or tokens printed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("FAIL account checks: " + ex.Message); return 1; }
        finally
        {
            if (factory is not null) await factory.DisposeAsync();
            SqlConnection.ClearAllPools();
            if (created) await new SqlCommand("ALTER DATABASE " + database + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE " + database, master).ExecuteNonQueryAsync();
            if (Path.GetFileName(folder) == "remarket-auth-" + run && Path.GetDirectoryName(folder) == Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) && Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }
    }
}
