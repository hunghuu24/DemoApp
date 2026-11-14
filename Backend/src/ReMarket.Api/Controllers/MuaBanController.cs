using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReMarket.Application.MuaBan;
using ReMarket.Application.SanPham;
using ReMarket.Application.TaiKhoan;
using ReMarket.Infrastructure.Commerce;

namespace ReMarket.Api.Controllers;

[ApiController, Authorize(Roles = "USER"), RequestSizeLimit(32 * 1024), Route("api/donhang")]
public sealed class DonHangController(MuaBanService service) : ControllerBase
{
    private Guid Id => Guid.Parse(User.FindFirstValue("sub")!);
    [HttpPost("baogia"), EnableRateLimiting("commerce")]
    public Task<BaoGiaDto> BaoGia(BaoGiaRequest request, CancellationToken ct) => service.BaoGiaAsync(Id, request, ct);
    [HttpPost, EnableRateLimiting("commerce")]
    public async Task<ActionResult<DonHangDto>> Tao(TaoDonRequest request, CancellationToken ct)
    { var item = await service.TaoDonAsync(Id, request, ct); return Created($"/api/donhang/{item.Id}", item); }
    [HttpGet]
    public Task<TrangDonDto> DanhSach([FromQuery, MaxLength(10)] string vaiTro = "mua", [FromQuery, MaxLength(40)] string? trangThai = null,
        [FromQuery, Range(1,100000)] int trang = 1, CancellationToken ct = default) => service.DanhSachDonAsync(Id, vaiTro, trangThai, trang, ct);
    [HttpGet("{id:guid}")]
    public Task<DonHangDto> Don(Guid id, CancellationToken ct) => service.DonAsync(id, Id, ct);
    [HttpPost("{id:guid}/hoithoai"), EnableRateLimiting("commerce")]
    public Task<HoiThoaiDto> Chat(Guid id, CancellationToken ct) => service.MoChatDonAsync(id, Id, ct);
    [HttpPost("{id:guid}/thanhtoan"), EnableRateLimiting("commerce")]
    public Task<DonHangDto> ThanhToan(Guid id, ThanhToanRequest request, CancellationToken ct) => service.ThanhToanAsync(id, Id, request, ct);
    [HttpPost("{id:guid}/huy"), EnableRateLimiting("commerce")]
    public Task<DonHangDto> Huy(Guid id, ChuyenDonRequest request, CancellationToken ct) => service.HuyAsync(id, Id, request.PhienBan, request.LyDo, false, ct);
    [HttpPost("{id:guid}/vanchuyen"), EnableRateLimiting("commerce")]
    public Task<DonHangDto> VanChuyen(Guid id, ChuyenShipRequest request, CancellationToken ct) => service.ChuyenShipAsync(id, Id, request, ct);
    [HttpPost("{id:guid}/nhanhang"), EnableRateLimiting("commerce")]
    public Task<DonHangDto> NhanHang(Guid id, PhienBanTinRequest request, CancellationToken ct) => service.NhanHangAsync(id, Id, request.PhienBan, ct);
    [HttpPost("{id:guid}/danhgia"), EnableRateLimiting("commerce")]
    public Task<DanhGiaMuaDto> DanhGia(Guid id, GuiDanhGiaRequest request, CancellationToken ct) => service.DanhGiaAsync(id, Id, request, ct);
    [HttpGet("{id:guid}/anh")]
    public async Task<IActionResult> Anh(Guid id, CancellationToken ct) => PrivateImage(await service.AnhDonAsync(id, Id, ct));
    private IActionResult PrivateImage(string file)
    { Response.Headers.CacheControl = "private, no-store"; Response.Headers.XContentTypeOptions = "nosniff"; return PhysicalFile(file, Path.GetExtension(file) == ".png" ? "image/png" : "image/jpeg"); }
}

[ApiController, Authorize(Roles = "USER"), RequestSizeLimit(24 * 1024), Route("api/hoithoai")]
public sealed class HoiThoaiController(MuaBanService service) : ControllerBase
{
    private Guid Id => Guid.Parse(User.FindFirstValue("sub")!);
    [HttpGet]
    public Task<List<HopThuMuaDto>> HopThu([FromQuery] bool chuaDoc = false, [FromQuery, Range(1,100000)] int trang = 1, CancellationToken ct = default) => service.HopThuAsync(Id, chuaDoc, trang, ct);
    [HttpPost, EnableRateLimiting("commerce")]
    public Task<HoiThoaiDto> Mo(MoChatRequest request, CancellationToken ct) => service.MoChatAsync(Id, request.TinDangId, ct);
    [HttpGet("{id:guid}")]
    public Task<HoiThoaiDto> Doc(Guid id, [FromQuery, Range(1,long.MaxValue)] long? truoc = null, CancellationToken ct = default) => service.ChatAsync(id, Id, truoc, ct);
    [HttpPost("{id:guid}/tinnhan"), EnableRateLimiting("commerce")]
    public Task<HoiThoaiDto> Gui(Guid id, GuiTinRequest request, CancellationToken ct) => service.GuiTinAsync(id, Id, request, ct);
    [HttpPost("{id:guid}/phieu"), EnableRateLimiting("commerce")]
    public Task<HoiThoaiDto> Phieu(Guid id, TaoPhieuRequest request, CancellationToken ct) => service.TaoPhieuAsync(id, Id, request, ct);
    [HttpPost("{id:guid}/phieu/{phieuId:guid}"), EnableRateLimiting("commerce")]
    public Task<HoiThoaiDto> PhanHoi(Guid id, Guid phieuId, PhanHoiPhieuRequest request, CancellationToken ct) => service.PhanHoiPhieuAsync(id, phieuId, Id, request, ct);
    [HttpPost("{id:guid}/anh"), RequestSizeLimit(5 * 1024 * 1024 + 16 * 1024), EnableRateLimiting("commerce")]
    public async Task<HoiThoaiDto> GuiAnh(Guid id, IFormFile anh, [FromForm] Guid maPhiaClient, CancellationToken ct)
    {
        if (anh.Length is 0 or > 5 * 1024 * 1024) throw new LoiTaiKhoan(400, "Ảnh JPG/PNG tối đa 5 MB.");
        await using var stream = anh.OpenReadStream(); return await service.GuiAnhAsync(id, Id, maPhiaClient, stream, ct);
    }
    [HttpGet("{id:guid}/anh/{tepId:guid}")]
    public async Task<IActionResult> Anh(Guid id, Guid tepId, CancellationToken ct) => PrivateImage(await service.AnhChatAsync(id, tepId, Id, ct));
    [HttpGet("{id:guid}/sanpham/anh")]
    public async Task<IActionResult> AnhMon(Guid id, CancellationToken ct) => PrivateImage(await service.AnhMonChatAsync(id, Id, ct));
    private IActionResult PrivateImage(string file)
    { Response.Headers.CacheControl = "private, no-store"; Response.Headers.XContentTypeOptions = "nosniff"; return PhysicalFile(file, Path.GetExtension(file) == ".png" ? "image/png" : "image/jpeg"); }
}

[ApiController, Authorize(Roles = "USER"), RequestSizeLimit(32 * 1024), Route("api/hotro")]
public sealed class HoTroController(MuaBanService service) : ControllerBase
{
    private Guid Id => Guid.Parse(User.FindFirstValue("sub")!);
    [HttpPost, EnableRateLimiting("commerce")]
    public async Task<ActionResult<HoTroDto>> Tao(TaoHoTroRequest request, CancellationToken ct)
    { var item = await service.TaoHoTroAsync(Id, request, ct); return Created($"/api/hotro/{item.Id}", item); }
    [HttpGet]
    public Task<List<HoTroDto>> List([FromQuery, Range(1,100000)] int trang = 1, CancellationToken ct = default) => service.DanhSachHoTroAsync(Id, false, trang, ct);
    [HttpGet("{id:guid}")]
    public Task<HoTroDto> Get(Guid id, CancellationToken ct) => service.HoTroAsync(id, Id, false, ct);
    [HttpPost("{id:guid}/phanhoi"), EnableRateLimiting("commerce")]
    public Task<HoTroDto> Gui(Guid id, PhanHoiHoTroRequest request, CancellationToken ct) => service.PhanHoiHoTroAsync(id, Id, false, request, ct);
}

[ApiController, Authorize(Roles = "ADMIN"), RequestSizeLimit(32 * 1024), Route("api/admin/hotro")]
public sealed class QuanTriHoTroController(MuaBanService service) : ControllerBase
{
    private Guid Id => Guid.Parse(User.FindFirstValue("sub")!);
    [HttpGet]
    public Task<List<HoTroDto>> List([FromQuery, Range(1,100000)] int trang = 1, CancellationToken ct = default) => service.DanhSachHoTroAsync(Id, true, trang, ct);
    [HttpGet("{id:guid}")]
    public Task<HoTroDto> Get(Guid id, CancellationToken ct) => service.HoTroAsync(id, Id, true, ct);
    [HttpPost("{id:guid}/phanhoi"), EnableRateLimiting("commerce")]
    public Task<HoTroDto> XuLy(Guid id, PhanHoiHoTroRequest request, CancellationToken ct) => service.PhanHoiHoTroAsync(id, Id, true, request, ct);
}
