using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Authentication;

public sealed class TokenService(IOptions<TuyChonJwt> options)
{
    public static string TaoBiMat() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    public static string Bam(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    public (string Token, DateTimeOffset HetHan) TaoAccess(PhienDangNhap session, string[] roles)
    {
        var settings = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(settings.AccessMinutes);
        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, session.NguoiDungId.ToString()),
            new(JwtRegisteredClaimNames.Jti, session.MaTruyCap),
            new("sid", session.Id.ToString())
        };
        claims.AddRange(roles.Select(role => new Claim("role", role)));
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            now.UtcDateTime, expires.UtcDateTime,
            new SigningCredentials(new SymmetricSecurityKey(settings.KhoaKy()), SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
