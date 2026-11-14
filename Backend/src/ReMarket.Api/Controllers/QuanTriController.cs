using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReMarket.Application.QuanTri;
using ReMarket.Application.SanPham;
using ReMarket.Infrastructure.QuanTri;

namespace ReMarket.Api.Controllers;
[ApiController,Authorize(Roles="ADMIN"),Route("api/admin"),RequestSizeLimit(32*1024)]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class QuanTriController(QuanTriService service):ControllerBase
{
    private Guid Actor=>Guid.Parse(User.FindFirstValue("sub")!);
    [HttpGet("bao-cao")] public Task<BaoCaoQuanTriDto> Report([FromQuery]DateOnly? tuNgay,[FromQuery]DateOnly? denNgay,CancellationToken ct)=>service.BaoCaoAsync(Actor,tuNgay,denNgay,ct);
    [HttpGet("nguoi-dung")] public Task<TrangQuanTri<NguoiDungQuanTriDto>> Users([FromQuery,MaxLength(100)]string? q,[FromQuery,MaxLength(40)]string? trangThai,[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.NguoiDungAsync(Actor,q,trangThai,trang,ct);
    [HttpGet("nguoi-dung/{id:guid}")] public Task<ChiTietNguoiDungQuanTriDto> UserDetail(Guid id,CancellationToken ct)=>service.NguoiDungAsync(Actor,id,ct);
    [HttpPost("nguoi-dung/{id:guid}/khoa"),EnableRateLimiting("commerce")] public Task<ChiTietNguoiDungQuanTriDto> Lock(Guid id,KhoaNguoiDungRequest r,CancellationToken ct)=>service.KhoaAsync(Actor,id,r,ct);
    [HttpPost("nguoi-dung/{id:guid}/mo-khoa"),EnableRateLimiting("commerce")] public Task<ChiTietNguoiDungQuanTriDto> Unlock(Guid id,MoKhoaNguoiDungRequest r,CancellationToken ct)=>service.MoKhoaAsync(Actor,id,r,ct);
    [HttpGet("danh-muc")] public Task<List<DanhMucQuanTriDto>> Categories(CancellationToken ct)=>service.DanhMucAsync(Actor,ct);
    [HttpPost("danh-muc"),EnableRateLimiting("commerce")] public async Task<ActionResult<DanhMucQuanTriDto>> Create(LuuDanhMucQuanTriRequest r,CancellationToken ct){var row=await service.DanhMucAsync(Actor,null,r,ct);return Created($"/api/admin/danh-muc/{row.Id}",row);}
    [HttpPut("danh-muc/{id:guid}"),EnableRateLimiting("commerce")] public Task<DanhMucQuanTriDto> Update(Guid id,LuuDanhMucQuanTriRequest r,CancellationToken ct)=>service.DanhMucAsync(Actor,id,r,ct);
    [HttpGet("san-pham")] public Task<TrangTinDto> Listings([FromQuery,MaxLength(100)]string? q,[FromQuery,MaxLength(40)]string? trangThai,[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.TinAsync(Actor,q,trangThai,trang,ct);
    [HttpGet("san-pham/{id:guid}")] public Task<ChiTietTinQuanTriDto> Listing(Guid id,CancellationToken ct)=>service.TinAsync(Actor,id,ct);
    [HttpPost("san-pham/{id:guid}/duyet"),EnableRateLimiting("commerce")] public Task<ChiTietTinQuanTriDto> Review(Guid id,DuyetTinRequest r,CancellationToken ct)=>service.DuyetAsync(Actor,id,r,ct);
    [HttpPost("san-pham/{id:guid}/an"),EnableRateLimiting("commerce")] public Task<ChiTietTinQuanTriDto> Hide(Guid id,AnTinQuanTriRequest r,CancellationToken ct)=>service.AnTinAsync(Actor,id,r,ct);
    [HttpGet("giao-dich")] public Task<TrangQuanTri<GiaoDichQuanTriDto>> Trades([FromQuery,MaxLength(100)]string? q,[FromQuery,MaxLength(40)]string? loai,[FromQuery,MaxLength(40)]string? trangThai,[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.GiaoDichAsync(Actor,q,loai,trangThai,trang,ct);
    [HttpGet("giao-dich/{id:guid}")] public Task<ChiTietGiaoDichQuanTriDto> Trade(Guid id,[FromQuery,Range(1,long.MaxValue)]long? truoc=null,CancellationToken ct=default)=>service.GiaoDichAsync(Actor,id,truoc,ct);
    [HttpGet("giao-dich/{id:guid}/media/{kind}/{file:guid}")] public async Task<IActionResult> Media(Guid id,string kind,Guid file,CancellationToken ct){var(path,mime)=await service.MediaAsync(Actor,id,kind,file,ct);Response.Headers.CacheControl="private, no-store";Response.Headers.XContentTypeOptions="nosniff";return PhysicalFile(path,mime,enableRangeProcessing:mime=="video/mp4");}
    [HttpGet("khieu-nai")] public Task<TrangQuanTri<KhieuNaiQuanTriDto>> Complaints([FromQuery,MaxLength(100)]string? q,[FromQuery,MaxLength(40)]string? trangThai,[FromQuery,Range(1,100000)]int trang=1,CancellationToken ct=default)=>service.KhieuNaiAsync(Actor,q,trangThai,trang,ct);
    [HttpPost("giao-dich/{id:guid}/khieu-nai"),EnableRateLimiting("commerce")] public async Task<ActionResult<ChiTietKhieuNaiQuanTriDto>> CreateComplaint(Guid id,TaoKhieuNaiQuanTriRequest r,CancellationToken ct){var row=await service.TaoKhieuNaiAsync(Actor,id,r,ct);return Created($"/api/admin/khieu-nai/{row.KhieuNai.Id}",row);}
    [HttpGet("khieu-nai/{id:guid}")] public Task<ChiTietKhieuNaiQuanTriDto> Complaint(Guid id,CancellationToken ct)=>service.KhieuNaiAsync(Actor,id,ct);
    [HttpPost("khieu-nai/{id:guid}/tiep-nhan"),EnableRateLimiting("commerce")] public Task<ChiTietKhieuNaiQuanTriDto> Accept(Guid id,TiepNhanKhieuNaiRequest r,CancellationToken ct)=>service.TiepNhanAsync(Actor,id,r,ct);
    [HttpPost("khieu-nai/{id:guid}/quyet-dinh"),EnableRateLimiting("commerce")] public Task<ChiTietKhieuNaiQuanTriDto> Decide(Guid id,QuyetDinhQuanTriRequest r,CancellationToken ct)=>service.QuyetDinhAsync(Actor,id,r,ct);
    [HttpPost("khieu-nai/{id:guid}/xac-minh"),EnableRateLimiting("commerce")] public Task<ChiTietKhieuNaiQuanTriDto> Verify(Guid id,XacMinhQuyetDinhRequest r,CancellationToken ct)=>service.XacMinhAsync(Actor,id,r,ct);
}
