using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReMarket.Application.TaiKhoan;
using ReMarket.Infrastructure.Authentication;

namespace ReMarket.Api.Controllers;

[ApiController, Route("api/taikhoan"), Authorize, RequestSizeLimit(16 * 1024)]
public sealed class TaiKhoanController(ITaiKhoanService accounts, AnhDaiDienService avatars) : ControllerBase
{
    private Guid Id => Guid.Parse(User.FindFirstValue("sub")!);

    [HttpPost("dangky"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<ActionResult<HoSoDto>> DangKy(DangKyRequest request, CancellationToken ct) =>
        Created("/api/taikhoan/toi", await accounts.DangKyAsync(request, ct));
    [HttpPost("dangnhap"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<DangNhapDto> DangNhap(DangNhapRequest request, CancellationToken ct) => await accounts.DangNhapAsync(request, ct);
    [HttpPost("lam-moi"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<DangNhapDto> LamMoi(RefreshRequest request, CancellationToken ct) => await accounts.LamMoiAsync(request.RefreshToken, ct);
    [HttpPost("dangxuat")]
    public async Task<IActionResult> DangXuat(CancellationToken ct)
    {
        await accounts.DangXuatAsync(Id, Guid.Parse(User.FindFirstValue("sid")!), ct);
        return NoContent();
    }
    [HttpPost("quenmatkhau"), AllowAnonymous, EnableRateLimiting("recovery")]
    public async Task<ActionResult<ThongBaoDto>> QuenMatKhau(QuenMatKhauRequest request, CancellationToken ct)
    {
        await accounts.QuenMatKhauAsync(request.Email, ct);
        return Accepted(new ThongBaoDto("Nếu email đã được đăng ký, bạn sẽ nhận được liên kết khôi phục có hiệu lực trong 15 phút."));
    }
    [HttpPost("datlaimatkhau"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauRequest request, CancellationToken ct)
    {
        await accounts.DatLaiMatKhauAsync(request, ct); return NoContent();
    }
    [HttpPost("doimatkhau"), EnableRateLimiting("auth")]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauRequest request, CancellationToken ct)
    {
        await accounts.DoiMatKhauAsync(Id, request, ct); return NoContent();
    }
    [HttpGet("toi")]
    public async Task<HoSoDto> Toi(CancellationToken ct) => await accounts.LayHoSoAsync(Id, ct);
    [HttpPut("toi")]
    public async Task<HoSoDto> CapNhat(CapNhatHoSoRequest request, CancellationToken ct) => await accounts.CapNhatHoSoAsync(Id, request, ct);
    [HttpPost("anh-dai-dien"), RequestSizeLimit(2 * 1024 * 1024 + 16 * 1024), EnableRateLimiting("auth")]
    public async Task<HoSoDto> DoiAnh(IFormFile anh, CancellationToken ct)
    {
        if (anh.Length is 0 or > 2 * 1024 * 1024) throw new LoiTaiKhoan(400, "Ảnh tối đa 2 MB.");
        await using var stream = anh.OpenReadStream();
        return await avatars.LuuAsync(Id, stream, ct);
    }
    [HttpGet("anh-dai-dien")]
    public async Task<IActionResult> Anh(CancellationToken ct)
    {
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return PhysicalFile(await avatars.DuongDanAsync(Id, ct), "image/jpeg");
    }
}
