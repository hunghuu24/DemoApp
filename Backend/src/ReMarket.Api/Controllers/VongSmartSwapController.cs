using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReMarket.Application.MuaBan;
using ReMarket.Application.SmartSwap;
using ReMarket.Infrastructure.Commerce;
using ReMarket.Infrastructure.SmartSwap;
using ReMarket.Application.TaiKhoan;

namespace ReMarket.Api.Controllers;

[ApiController,Authorize(Roles="USER"),Route("api/smart-swaps"),RequestSizeLimit(32*1024)]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class VongSmartSwapController(VongSmartSwapService service,MuaBanService chat):ControllerBase
{
    private Guid Id=>Guid.Parse(User.FindFirstValue("sub")!);
    [HttpPost("{id:guid}/evidence"),RequestSizeLimit(5*1024*1024+16*1024),EnableRateLimiting("commerce")]
    public async Task<GiaoNhanSmartSwapDto> BangChung(Guid id,IFormFile anh,[FromForm]Guid maYeuCau,CancellationToken ct)
    {if(anh.Length is 0 or >5*1024*1024)throw new LoiTaiKhoan(400,"Ảnh JPG/PNG tối đa 5 MB.");await using var input=anh.OpenReadStream();return await service.BangChungAsync(id,Id,maYeuCau,input,ct);}
    [HttpGet("{id:guid}/evidence/{file:guid}")]
    public async Task<IActionResult> AnhBangChung(Guid id,Guid file,CancellationToken ct)
    {var path=await service.AnhBangChungAsync(id,Id,file,ct);Response.Headers.CacheControl="private, no-store";Response.Headers.XContentTypeOptions="nosniff";return PhysicalFile(path,Path.GetExtension(path)==".png"?"image/png":"image/jpeg");}
    [HttpGet("{id:guid}/reviews")]
    public Task<List<DanhGiaSmartSwapDto>> DanhGiaCuaToi(Guid id,CancellationToken ct)=>service.DanhGiaCuaToiAsync(id,Id,ct);
    [HttpPost("{id:guid}/reviews"),EnableRateLimiting("commerce")]
    public Task<List<DanhGiaSmartSwapDto>> DanhGia(Guid id,DanhGiaSmartSwapRequest r,CancellationToken ct)=>service.DanhGiaAsync(id,Id,r,ct);
    [HttpPost("preview"),EnableRateLimiting("smart-swap-search")]
    public Task<XemVongSmartSwapDto> Preview(XemVongSmartSwapRequest r,CancellationToken ct)=>service.XemAsync(Id,r,ct);
    [HttpPost("invitations"),EnableRateLimiting("commerce")]
    public async Task<ActionResult<ChiTietVongSmartSwapDto>> Moi(GuiLoiMoiSmartSwapRequest r,CancellationToken ct)
    {var row=await service.MoiAsync(Id,r,ct);return Created($"/api/smart-swaps/{row.Id}",row);}
    [HttpGet]
    public Task<TrangVongSmartSwapDto> List([FromQuery,MaxLength(20)]string tab="tatca",[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.DanhSachAsync(Id,tab,trang,ct);
    [HttpGet("{id:guid}")]
    public Task<ChiTietVongSmartSwapDto> Detail(Guid id,CancellationToken ct)=>service.DocAsync(id,Id,ct);
    [HttpPost("{id:guid}/responses"),EnableRateLimiting("commerce")]
    public Task<ChiTietVongSmartSwapDto> PhanHoi(Guid id,PhanHoiSmartSwapRequest r,CancellationToken ct)=>service.PhanHoiAsync(id,Id,r,ct);
    [HttpPost("{id:guid}/proposals"),EnableRateLimiting("commerce")]
    public Task<ChiTietVongSmartSwapDto> DeXuat(Guid id,TaoPhuongAnSmartSwapRequest r,CancellationToken ct)=>service.DeXuatAsync(id,Id,r,ct);
    [HttpGet("{id:guid}/versions")]
    public async Task<List<PhuongAnSmartSwapDto>> LichSu(Guid id,CancellationToken ct)=>(await service.DocAsync(id,Id,ct)).LichSu;
    [HttpPost("{id:guid}/cancel"),EnableRateLimiting("commerce")]
    public Task<ChiTietVongSmartSwapDto> Huy(Guid id,HuyVongSmartSwapRequest r,CancellationToken ct)=>service.HuyAsync(id,Id,r,ct);
    [HttpGet("{id:guid}/chat")]
    public async Task<HoiThoaiDto> Chat(Guid id,CancellationToken ct)=>await chat.ChatAsync(await service.ChatAsync(id,Id,ct),Id,null,ct);
    [HttpGet("{id:guid}/products/{productId:guid}/image")]
    public async Task<IActionResult> Image(Guid id,Guid productId,CancellationToken ct)
    {var path=await service.AnhAsync(id,productId,Id,ct);Response.Headers.CacheControl="private, no-store";Response.Headers.XContentTypeOptions="nosniff";return PhysicalFile(path,Path.GetExtension(path)==".png"?"image/png":"image/jpeg");}
    [HttpGet("notifications")]
    public Task<List<ThongBaoSmartSwapDto>> ThongBao([FromQuery]bool chuaDoc=false,[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.ThongBaoAsync(Id,chuaDoc,trang,ct);
    [HttpPost("notifications/{id:guid}/read"),EnableRateLimiting("commerce")]
    public async Task<IActionResult> DocThongBao(Guid id,CancellationToken ct){await service.DocThongBaoAsync(id,Id,ct);return NoContent();}
    [HttpGet("{id:guid}/delivery")]
    public Task<GiaoNhanSmartSwapDto> GiaoNhan(Guid id,CancellationToken ct)=>service.GiaoNhanAsync(id,Id,ct);
    [HttpPut("{id:guid}/delivery/contact"),EnableRateLimiting("commerce")]
    public Task<GiaoNhanSmartSwapDto> LienHe(Guid id,XacNhanLienHeSmartSwapRequest r,CancellationToken ct)=>service.LienHeAsync(id,Id,r,ct);
    [HttpPost("{id:guid}/shipments"),EnableRateLimiting("commerce")]
    public Task<GiaoNhanSmartSwapDto> TaoVanChuyen(Guid id,TaoVanChuyenSmartSwapRequest r,CancellationToken ct)=>service.TaoVanChuyenAsync(id,Id,r,ct);
    [HttpPut("{id:guid}/shipments/{shipmentId:guid}/tracking"),EnableRateLimiting("commerce")]
    public Task<GiaoNhanSmartSwapDto> MaVanDon(Guid id,Guid shipmentId,MaVanDonSmartSwapRequest r,CancellationToken ct)=>service.MaVanDonAsync(id,shipmentId,Id,r,ct);
    [HttpPost("{id:guid}/shipments/{shipmentId:guid}/status"),EnableRateLimiting("commerce")]
    public Task<GiaoNhanSmartSwapDto> ChuyenVanChuyen(Guid id,Guid shipmentId,ChuyenVanChuyenSmartSwapRequest r,CancellationToken ct)=>service.ChuyenVanChuyenAsync(id,shipmentId,Id,r,ct);
    [HttpPost("{id:guid}/shipments/{shipmentId:guid}/received"),EnableRateLimiting("commerce")]
    public Task<GiaoNhanSmartSwapDto> NhanDung(Guid id,Guid shipmentId,NhanHangSmartSwapRequest r,CancellationToken ct)=>service.NhanDungHangAsync(id,shipmentId,Id,r,ct);
    [HttpPost("{id:guid}/issues"),EnableRateLimiting("commerce")]
    public Task<GiaoNhanSmartSwapDto> BaoSuCo(Guid id,BaoSuCoSmartSwapRequest r,CancellationToken ct)=>service.BaoSuCoAsync(id,Id,r,ct);
}
