using Microsoft.EntityFrameworkCore;
using ReMarket.Application.HeThong;
using ReMarket.Infrastructure.Persistence;
using ReMarket.Infrastructure.Seeding;
using ReMarket.Api.Authentication;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Catalog;
using ReMarket.Infrastructure.Commerce;
using ReMarket.Infrastructure.Exchange;
using ReMarket.Application.SmartSwap;
using ReMarket.Infrastructure.SmartSwap;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ReMarketDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("ReMarket")
    ?? throw new InvalidOperationException("Chua cau hinh ConnectionStrings:ReMarket.")));
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
builder.Services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
    new BadRequestObjectResult(new {
        title = "Dữ liệu chưa hợp lệ", detail = "Vui lòng kiểm tra các trường thông tin và thử lại.", status = 400,
        errors = context.ModelState.Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(_ => "Giá trị không hợp lệ.").ToArray())
    }));
builder.Services.ThemXacThuc(builder.Configuration);
builder.Services.AddScoped<TinDangService>();
builder.Services.AddScoped<KhoAnhSanPham>();
builder.Services.AddScoped<KhoaTonKho>();
builder.Services.AddScoped<MuaBanService>();
builder.Services.AddScoped<TraoDoiService>();
builder.Services.AddScoped<TimSmartSwapService>();
builder.Services.AddScoped<VongSmartSwapService>();
builder.Services.AddScoped<DemoGiaoNhanSmartSwap>();
builder.Services.AddScoped<ReMarket.Infrastructure.QuanTri.QuanTriService>();
if(builder.Configuration.GetValue("Admin:EnableUnlockWorker",true))builder.Services.AddHostedService<MoKhoaHetHanWorker>();
builder.Services.AddOptions<GioiHanTimSmartSwap>().Bind(builder.Configuration.GetSection("SmartSwapSearch"))
    .Validate(x => x.SoTinToiDa is >=3 and <=5000 && x.SoCanhToiDa is >=3 and <=1000000 && x.SoLuotDuyetToiDa is >=1 and <=20000000,
        "Gioi han tim Smart Swap khong hop le.").ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
if (builder.Configuration.GetValue("Commerce:EnableExpirationWorker", true)) builder.Services.AddHostedService<HetHanDonHangWorker>();
if (builder.Configuration.GetValue("Exchange:EnableExpirationWorker", true)) builder.Services.AddHostedService<HetHanTraoDoiWorker>();
if (builder.Configuration.GetValue("SmartSwap:EnableExpirationWorker", true)) builder.Services.AddHostedService<HetHanSmartSwapWorker>();
builder.Services.AddExceptionHandler<XuLyLoiTaiKhoan>();
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    foreach (var policy in new[] { "auth", "recovery", "listing-write", "commerce", "smart-swap-search" })
        options.AddPolicy(policy, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "local",
            _ => new FixedWindowRateLimiterOptions {
                PermitLimit = builder.Configuration.GetValue("RateLimits:PermitLimit", policy == "recovery" ? 5 : policy == "commerce" ? 60 : policy == "smart-swap-search" ? 10 : 20),
                Window = policy == "recovery" ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(1), QueueLimit = 0
            }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
    });
});
builder.Services.AddProblemDetails();
var app = builder.Build();

// Explicit setup commands only; normal server startup never migrates or seeds.
if (args.Contains("--migrate") || args.Contains("--seed-demo") || args.Contains("--seed-smart-delivery-demo") || args.Contains("--grant-admin"))
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException("Lenh khoi tao/demo chi duoc phep trong Development.");
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ReMarketDbContext>();
    if (args.Contains("--migrate")) await db.Database.MigrateAsync();
    if (args.Contains("--seed-demo")) await DuLieuMau.KhoiTaoAsync(db);
    if (args.Contains("--seed-smart-delivery-demo"))
    {
        await DuLieuMau.KhoiTaoAsync(db);
        var id=await scope.ServiceProvider.GetRequiredService<DemoGiaoNhanSmartSwap>().KhoiTaoAsync();
        Console.WriteLine("Demo Smart Swap delivery ready; existing progress preserved. CircleId="+id);
    }
    if (args.Contains("--grant-admin"))
    {
        var at = Array.IndexOf(args, "--grant-admin");
        var email = at + 1 < args.Length ? args[at + 1].Trim().ToUpperInvariant() : throw new InvalidOperationException("Can cung cap email.");
        var user = await db.NguoiDung.SingleOrDefaultAsync(x => x.EmailChuanHoa == email)
            ?? throw new InvalidOperationException("Tai khoan phai dang ky truoc khi cap quyen.");
        var role = await db.VaiTro.SingleAsync(x => x.Ma == "ADMIN");
        if (!await db.NguoiDungVaiTro.AnyAsync(x => x.NguoiDungId == user.Id && x.VaiTroId == role.Id))
        {
            db.Add(new NguoiDungVaiTro { NguoiDungId = user.Id, VaiTroId = role.Id });
            await db.SaveChangesAsync();
        }
        Console.WriteLine("Da cap quyen ADMIN trong co so du lieu Development.");
    }
    return;
}

app.UseExceptionHandler();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapGet("/health/live", () => Results.Ok(new { trangThai = "DangChay" }));
app.MapGet("/health/ready", async (ReMarketDbContext db, CancellationToken ct) =>
    await CoSoDuLieuSanSangAsync(db, ct) ? Results.Ok() : Results.StatusCode(503));
app.MapGet("/api/hethong/trangthai", async (ReMarketDbContext db, CancellationToken ct) =>
    new TrangThaiHeThongDto("ReMarket", "0.1.0", await CoSoDuLieuSanSangAsync(db, ct), DateTimeOffset.UtcNow))
    .WithName("LayTrangThaiHeThong");
app.MapControllers();
app.Run();

static async Task<bool> CoSoDuLieuSanSangAsync(ReMarketDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) && !(await db.Database.GetPendingMigrationsAsync(ct)).Any();

public partial class Program;
