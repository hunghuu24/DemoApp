using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SanPham;
using ReMarket.Application.TaiKhoan;
using ReMarket.Infrastructure.Persistence;
using ReMarket.Infrastructure.Catalog;

namespace ReMarket.Api.Controllers;

[ApiController, Route("api/tindang"), Authorize(Roles = "USER"), RequestSizeLimit(32 * 1024)]
public sealed class TinDangController(TinDangService listings) : ControllerBase
{
    private Guid Id => Guid.Parse(User.FindFirstValue("sub")!);
    private Guid? Viewer => Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : null;
    [HttpGet, AllowAnonymous]
    public Task<TrangTinDto> Tim([FromQuery] BoLocTin filter, CancellationToken ct) => listings.TimAsync(filter, ct);
    [HttpGet("danhmuc"), AllowAnonymous]
    public Task<List<DanhMucDto>> DanhMuc(CancellationToken ct) => listings.DanhMucAsync(ct);
    [HttpGet("trangchu"), AllowAnonymous]
    public Task<TrangChuSanPhamDto> TrangChu(CancellationToken ct) => listings.TrangChuAsync(ct);
    [HttpGet("{id:guid}"), AllowAnonymous]
    public Task<TinDangDto> ChiTiet(Guid id, CancellationToken ct) => listings.ChiTietAsync(id, Viewer, false, ct);
    [HttpGet("cuatoi")]
    public Task<TinCuaToiDto> CuaToi([FromQuery, MaxLength(40)] string? trangThai,
        [FromQuery, Range(1, 100000)] int trang = 1, [FromQuery, Range(1, 50)] int kichThuocTrang = 12, CancellationToken ct = default,
        [FromQuery, MaxLength(40)] string loaiTin="MuaBan") => listings.CuaToiAsync(Id, trangThai, trang, kichThuocTrang, ct,loaiTin);
    [HttpGet("{id:guid}/quanly")]
    public Task<TinDangDto> QuanLy(Guid id, CancellationToken ct) => listings.ChiTietAsync(id, Id, true, ct);
    [HttpPost, EnableRateLimiting("listing-write")]
    public async Task<ActionResult<TinDangDto>> Tao(LuuTinDangRequest form, CancellationToken ct)
    {
        var listing = await listings.TaoAsync(Id, form, ct);
        return Created($"/api/tindang/{listing.Id}/quanly", listing);
    }
    [HttpPut("{id:guid}"), EnableRateLimiting("listing-write")]
    public Task<TinDangDto> Sua(Guid id, LuuTinDangRequest form, CancellationToken ct) => listings.SuaAsync(id, Id, form, ct);
    [HttpPost("{id:guid}/gui-duyet"), EnableRateLimiting("listing-write")]
    public Task<TinDangDto> GuiDuyet(Guid id, PhienBanTinRequest form, CancellationToken ct) => listings.ChuyenAsync(id, Id, form.PhienBan, "gui-duyet", ct);
    [HttpPost("{id:guid}/an"), EnableRateLimiting("listing-write")]
    public Task<TinDangDto> An(Guid id, PhienBanTinRequest form, CancellationToken ct) => listings.ChuyenAsync(id, Id, form.PhienBan, "an", ct);
    [HttpPost("{id:guid}/da-ban"), EnableRateLimiting("listing-write")]
    public Task<TinDangDto> DaBan(Guid id, PhienBanTinRequest form, CancellationToken ct) => listings.ChuyenAsync(id, Id, form.PhienBan, "da-ban", ct);
    [HttpDelete("{id:guid}"), EnableRateLimiting("listing-write")]
    public async Task<IActionResult> Xoa(Guid id, [FromBody] PhienBanTinRequest form, CancellationToken ct)
    { await listings.ChuyenAsync(id, Id, form.PhienBan, "xoa", ct); return NoContent(); }
    [HttpPost("{id:guid}/anh"), RequestSizeLimit(5 * 1024 * 1024 + 32 * 1024), EnableRateLimiting("listing-write")]
    public async Task<TinDangDto> ThemAnh(Guid id, IFormFile anh, [FromForm, Required, MaxLength(32)] string phienBan, CancellationToken ct)
    {
        if (anh.Length is 0 or > 5 * 1024 * 1024) throw new LoiTaiKhoan(400, "Ảnh tối đa 5 MB.");
        await using var stream = anh.OpenReadStream();
        return await listings.ThemAnhAsync(id, Id, phienBan, stream, ct);
    }
    [HttpDelete("{id:guid}/anh/{anhId:guid}"), EnableRateLimiting("listing-write")]
    public Task<TinDangDto> XoaAnh(Guid id, Guid anhId, [FromBody] PhienBanTinRequest form, CancellationToken ct) => listings.SuaAnhAsync(id, anhId, Id, form.PhienBan, true, ct);
    [HttpPut("{id:guid}/anh/{anhId:guid}/bia"), EnableRateLimiting("listing-write")]
    public Task<TinDangDto> AnhBia(Guid id, Guid anhId, PhienBanTinRequest form, CancellationToken ct) => listings.SuaAnhAsync(id, anhId, Id, form.PhienBan, false, ct);
    [HttpGet("{id:guid}/anh/{anhId:guid}"), AllowAnonymous]
    public async Task<IActionResult> Anh(Guid id, Guid anhId, CancellationToken ct)
    {
        // A supplied expired/invalid bearer must challenge, so Coil's authenticator can refresh it.
        if (Request.Headers.Authorization.Count > 0 && User.Identity?.IsAuthenticated != true) return Challenge();
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        var file = await listings.AnhAsync(id, anhId, Viewer, User.IsInRole("ADMIN"), ct);
        return PhysicalFile(file, Path.GetExtension(file) == ".png" ? "image/png" : "image/jpeg");
    }
}

[ApiController, Route("api/admin/tindang"), Authorize(Roles = "ADMIN")]
public sealed class DuyetTinController(TinDangService listings) : ControllerBase
{
    [HttpGet]
    public Task<TrangTinDto> ChoDuyet([FromQuery, Range(1, 100000)] int trang = 1,
        [FromQuery, Range(1, 50)] int kichThuocTrang = 12, CancellationToken ct = default) => listings.ChoDuyetAsync(trang, kichThuocTrang, ct);
    [HttpPost("{id:guid}/duyet"), RequestSizeLimit(8 * 1024)]
    public Task<TinDangDto> Duyet(Guid id, DuyetTinRequest form, CancellationToken ct) => listings.DuyetAsync(id, Guid.Parse(User.FindFirstValue("sub")!), form, ct);
}

// Makes the existing bottom destination useful without claiming a realtime chat implementation.
[ApiController, Route("api/hopthu"), Authorize]
public sealed class HopThuController(ReMarketDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DanhSach(CancellationToken ct)
    {
        var id = Guid.Parse(User.FindFirstValue("sub")!);
        return Ok(await db.ThanhVienTroChuyen.Where(m => m.NguoiDungId == id).OrderByDescending(m => m.CuocTroChuyen.NgayTao).Take(50)
            .Select(m => new { id = m.CuocTroChuyenId, tieuDe = m.CuocTroChuyen.TieuDe,
                tinCuoi = db.TinNhan.Where(t => t.CuocTroChuyenId == m.CuocTroChuyenId).OrderByDescending(t => t.NgayTao).Select(t => t.NoiDung).FirstOrDefault(),
                ngayTao = m.CuocTroChuyen.NgayTao }).ToListAsync(ct));
    }
}
