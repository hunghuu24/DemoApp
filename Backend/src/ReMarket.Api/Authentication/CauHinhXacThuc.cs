using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Authentication;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Api.Authentication;

public static class CauHinhXacThuc
{
    public static IServiceCollection ThemXacThuc(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TuyChonJwt>().Bind(configuration.GetSection("Jwt"))
            .Validate(x => x.KhoaKy().Length >= 32, "Invalid JWT signing key").ValidateOnStart();
        services.Configure<PasswordHasherOptions>(x => x.IterationCount = 210_000);
        services.AddScoped<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();
        services.AddScoped<TokenService>();
        services.AddScoped<ITaiKhoanService, TaiKhoanService>();
        services.AddScoped<IGuiThuKhoiPhuc, GuiThuKhoiPhuc>();
        services.AddScoped<AnhDaiDienService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).Configure<IOptions<TuyChonJwt>>((options, jwt) =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true, ValidIssuer = jwt.Value.Issuer,
                ValidateAudience = true, ValidAudience = jwt.Value.Audience,
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(jwt.Value.KhoaKy()),
                ValidateLifetime = true, RequireExpirationTime = true, RequireSignedTokens = true,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = "sub", RoleClaimType = "role"
            };
            options.Events = new JwtBearerEvents {
                OnTokenValidated = async context =>
                {
                    if (!Guid.TryParse(context.Principal?.FindFirstValue("sub"), out var id) ||
                        !Guid.TryParse(context.Principal?.FindFirstValue("sid"), out var sid))
                    { context.Fail("Invalid session"); return; }
                    var jti = context.Principal.FindFirstValue("jti");
                    var db = context.HttpContext.RequestServices.GetRequiredService<ReMarketDbContext>();
                    var ct = context.HttpContext.RequestAborted;
                    var valid = await db.PhienDangNhap.AsNoTracking().AnyAsync(x => x.Id == sid && x.NguoiDungId == id &&
                        x.MaTruyCap == jti && x.NgayThuHoi == null && x.NgayHetHan > DateTimeOffset.UtcNow &&
                        x.NguoiDung.TrangThai == TrangThaiNguoiDung.HoatDong, ct);
                    if (!valid) { context.Fail("Session expired or revoked"); return; }
                    // Authorization always reflects current SQL roles, including role removal.
                    var roles = await db.NguoiDungVaiTro.Where(x => x.NguoiDungId == id).Select(x => x.VaiTro.Ma).ToListAsync(ct);
                    if (context.Principal.Identity is not ClaimsIdentity identity || roles.Count == 0)
                    { context.Fail("No role"); return; }
                    foreach (var role in identity.FindAll("role").ToArray()) identity.RemoveClaim(role);
                    identity.AddClaims(roles.Select(x => new Claim("role", x)));
                }
            };
        });
        services.AddAuthorization();
        return services;
    }
}
