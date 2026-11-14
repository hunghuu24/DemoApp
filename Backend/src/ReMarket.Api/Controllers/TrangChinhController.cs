using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReMarket.Domain.Enums;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Api.Controllers;

[ApiController]
public sealed class TrangChinhController(ReMarketDbContext db, ReMarket.Infrastructure.Catalog.TinDangService listings) : ControllerBase
{
    [HttpGet("api/trangchu"), Authorize(Roles = "USER")]
    public async Task<IActionResult> TrangChu(CancellationToken ct)
    {
        var data = await listings.TrangChuAsync(ct);
        return Ok(new {
            danhMuc = data.DanhMuc.Where(x => x.DanhMucChaId == null).Select(x => new { x.Id, x.Ten }),
            sanPham = data.MoiDang.Select(x => new { x.Id, x.Ten, x.GiaBan, x.TinhThanh })
        });
    }
    [HttpGet("api/admin/tongquan"), Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> TongQuan(CancellationToken ct) => Ok(new {
        tongNguoiDung = await db.NguoiDung.CountAsync(ct),
        tinChoDuyet = await db.TinDang.CountAsync(x => x.TrangThai == TrangThaiTinDang.ChoDuyet, ct),
        muaBan = await db.GiaoDich.CountAsync(x => x.Loai == LoaiGiaoDich.MuaBan, ct),
        traoDoi = await db.GiaoDich.CountAsync(x => x.Loai != LoaiGiaoDich.MuaBan, ct),
        tranhChap = await db.KhieuNai.CountAsync(x => x.TrangThai != TrangThaiKhieuNai.DaGiaiQuyet && x.TrangThai != TrangThaiKhieuNai.TuChoi, ct)
    });
}
