using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReMarket.Application.MuaBan;
using ReMarket.Application.SanPham;
using ReMarket.Application.TaiKhoan;
using ReMarket.Application.TraoDoi;
using ReMarket.Infrastructure.Commerce;
using ReMarket.Infrastructure.Exchange;

namespace ReMarket.Api.Controllers;

[ApiController,Authorize(Roles="USER"),Route("api/traodoi"),RequestSizeLimit(32*1024)]
public sealed class TraoDoiController(TraoDoiService service,MuaBanService chat):ControllerBase
{
    private Guid Id=>Guid.Parse(User.FindFirstValue("sub")!);
    [HttpGet("moncuatoi")]
    public Task<TrangTinDto> Mon([FromQuery,MaxLength(100)] string? tuKhoa,[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.MonCuaToiAsync(Id,tuKhoa,trang,ct);
    [HttpGet("tieuchi")]
    public Task<List<TieuChiTraoDoiDto>> TieuChi(CancellationToken ct)=>service.TieuChiAsync(ct);
    [HttpGet]
    public Task<TrangDeNghiDto> DanhSach([FromQuery,MaxLength(30)]string tab="gui",[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.DanhSachAsync(Id,tab,trang,ct);
    [HttpPost,EnableRateLimiting("commerce")]
    public async Task<ActionResult<DeNghiTraoDoiDto>> Tao(TaoDeNghiRequest r,CancellationToken ct){var result=await service.TaoAsync(Id,r,ct);return Created($"/api/traodoi/{result.Id}",result);}
    [HttpGet("{id:guid}")]
    public Task<DeNghiTraoDoiDto> Doc(Guid id,CancellationToken ct)=>service.DocAsync(id,Id,ct);
    [HttpPost("{id:guid}/phanhoi"),EnableRateLimiting("commerce")]
    public Task<DeNghiTraoDoiDto> PhanHoi(Guid id,PhanHoiDeNghiRequest r,CancellationToken ct)=>service.PhanHoiAsync(id,Id,r,ct);
    [HttpPost("{id:guid}/dexuatlai"),EnableRateLimiting("commerce")]
    public Task<DeNghiTraoDoiDto> DeXuatLai(Guid id,DoiDeNghiRequest r,CancellationToken ct)=>service.DeXuatLaiAsync(id,Id,r,ct);
    [HttpPost("{id:guid}/huy"),EnableRateLimiting("commerce")]
    public Task<DeNghiTraoDoiDto> Huy(Guid id,HuyDeNghiRequest r,CancellationToken ct)=>service.KetThucAsync(id,Id,r.PhienBan,r.LyDo,false,false,ct);
    [HttpPost("{id:guid}/giaonhan"),EnableRateLimiting("commerce")]
    public Task<DeNghiTraoDoiDto> GiaoNhan(Guid id,GiaoNhanTraoDoiRequest r,CancellationToken ct)=>service.GiaoNhanAsync(id,Id,r,ct);
    [HttpPost("{id:guid}/danhgia"),EnableRateLimiting("commerce")]
    public Task<DeNghiTraoDoiDto> DanhGia(Guid id,DanhGiaTraoDoiRequest r,CancellationToken ct)=>service.DanhGiaAsync(id,Id,r,ct);
    [HttpPost("{id:guid}/hoithoai"),EnableRateLimiting("commerce")]
    public async Task<HoiThoaiDto> Chat(Guid id,CancellationToken ct)=>await chat.ChatAsync(await service.ChatAsync(id,Id,ct),Id,null,ct);
    [HttpGet("{id:guid}/anh/{sanPhamId:guid}")]
    public async Task<IActionResult> Anh(Guid id,Guid sanPhamId,CancellationToken ct)=>PrivateImage(await service.AnhAsync(id,sanPhamId,Id,ct));
    [HttpPost("{id:guid}/bangchung"),RequestSizeLimit(5*1024*1024+16*1024),EnableRateLimiting("commerce")]
    public async Task<DeNghiTraoDoiDto> BangChung(Guid id,IFormFile anh,[FromForm]Guid maYeuCau,CancellationToken ct)
    {
        if(anh.Length is 0 or >5*1024*1024)throw new LoiTaiKhoan(400,"Ảnh JPG/PNG tối đa 5 MB.");
        await using var input=anh.OpenReadStream();return await service.BangChungAsync(id,Id,maYeuCau,input,ct);
    }
    [HttpGet("{id:guid}/bangchung/{fileId:guid}")]
    public async Task<IActionResult> DocBangChung(Guid id,Guid fileId,CancellationToken ct)=>PrivateImage(await service.DocBangChungAsync(id,Id,fileId,ct));
    private IActionResult PrivateImage(string path){Response.Headers.CacheControl="private, no-store";Response.Headers.XContentTypeOptions="nosniff";return PhysicalFile(path,Path.GetExtension(path)==".png"?"image/png":"image/jpeg");}
}
