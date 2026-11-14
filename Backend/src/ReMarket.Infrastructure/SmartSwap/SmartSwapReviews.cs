using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed partial class VongSmartSwapService
{
    public async Task<List<DanhGiaSmartSwapDto>> DanhGiaCuaToiAsync(Guid id,Guid actor,CancellationToken ct)
    {
        var v=await CuaToiAsync(id,actor,ct);
        return await (from r in db.DanhGia.AsNoTracking() join h in db.HoSoCaNhan on r.NguoiDuocDanhGiaId equals h.NguoiDungId
            where r.GiaoDichId==v.GiaoDichId && r.NguoiDanhGiaId==actor orderby r.NgayTao,r.Id
            select new DanhGiaSmartSwapDto(r.Id,r.NguoiDuocDanhGiaId,h.HoTen,r.SoSao,r.NoiDung,r.NgayTao)).ToListAsync(ct);
    }
    public async Task<List<DanhGiaSmartSwapDto>> DanhGiaAsync(Guid id,Guid actor,DanhGiaSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.MaYeuCau);
        if(r.DanhGia.Count!=2 || r.DanhGia.Select(x=>x.NguoiDuocDanhGiaId).Distinct().Count()!=2 ||
           r.DanhGia.Any(x=>x.SoSao is <1 or >5 || x.NoiDung.Trim().Length<5))throw new LoiTaiKhoan(400,"Đánh giá đủ hai thành viên, chọn 1–5 sao và nhận xét ít nhất 5 ký tự.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,_)=await KhoaAsync(id,actor,ct);
        if(v.TrangThai!=TrangThaiVong.HoanTat || !await db.GiaoDich.AnyAsync(x=>x.Id==v.GiaoDichId&&x.TrangThai==TrangThaiGiaoDich.HoanTat,ct))
            throw new LoiTaiKhoan(409,"Chỉ đánh giá khi cả ba đã xác nhận nhận đúng hàng và vòng hoàn tất.");
        var partners=await db.ThanhVienVong.Where(x=>x.VongSmartSwapId==id&&x.NguoiDungId!=actor).Select(x=>x.NguoiDungId).ToListAsync(ct);
        if(!partners.Order().SequenceEqual(r.DanhGia.Select(x=>x.NguoiDuocDanhGiaId).Order()))throw new LoiTaiKhoan(400,"Chỉ đánh giá hai thành viên còn lại trong vòng.");
        foreach(var item in r.DanhGia)
        {
            var old=await db.DanhGia.SingleOrDefaultAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.NguoiDanhGiaId==actor&&x.NguoiDuocDanhGiaId==item.NguoiDuocDanhGiaId,ct);
            if(old!=null){if(old.MaYeuCau!=r.MaYeuCau||old.SoSao!=item.SoSao||old.NoiDung!=item.NoiDung.Trim())throw new LoiTaiKhoan(409,"Bạn đã đánh giá vòng này.");}
            else db.Add(new DanhGia{GiaoDichId=v.GiaoDichId,NguoiDanhGiaId=actor,NguoiDuocDanhGiaId=item.NguoiDuocDanhGiaId,
                SoSao=item.SoSao,NoiDung=item.NoiDung.Trim(),MaYeuCau=r.MaYeuCau,NgayTao=Now});
        }
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DanhGiaCuaToiAsync(id,actor,ct);
    }
}
