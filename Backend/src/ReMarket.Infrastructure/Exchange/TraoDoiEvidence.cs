using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.TaiKhoan;
using ReMarket.Application.TraoDoi;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Exchange;

public sealed partial class TraoDoiService
{
    public async Task<DeNghiTraoDoiDto> BangChungAsync(Guid id,Guid actor,Guid key,Stream input,CancellationToken ct)
    {
        Key(key);await CuaToiAsync(id,actor,ct);
        using var buffer=new MemoryStream();var chunk=new byte[16384];
        while(true){var n=await input.ReadAsync(chunk,ct);if(n==0)break;if(buffer.Length+n>5*1024*1024)throw new LoiTaiKhoan(400,"Ảnh tối đa 5 MB.");await buffer.WriteAsync(chunk.AsMemory(0,n),ct);}
        var hash=Convert.ToHexString(SHA256.HashData(buffer.ToArray()));buffer.Position=0;
        var path=await images.LuuAsync(buffer,ct);
        try
        {
            await using var tx=await db.Database.BeginTransactionAsync(ct);var(o,_,_)=await KhoaAsync(id,actor,ct);
            var receipt=await db.XacNhanGiaoNhan.SingleOrDefaultAsync(x=>x.GiaoDichId==o.GiaoDichId&&x.NguoiXacNhanId==actor&&x.Loai==LoaiXacNhan.DaNhan,ct)??throw new LoiTaiKhoan(409,"Cần xác nhận giao nhận trước khi gắn bằng chứng.");
            var old=await db.BangChungGiaoNhan.SingleOrDefaultAsync(x=>x.XacNhanGiaoNhanId==receipt.Id&&x.MaYeuCau==key,ct);
            if(old!=null)
            {
                if(old.BamNoiDung!=hash)throw new LoiTaiKhoan(409,"Mã ảnh đã dùng cho nội dung khác.");images.Xoa(path);
            }
            else
            {
                if(await db.BangChungGiaoNhan.CountAsync(x=>x.XacNhanGiaoNhanId==receipt.Id,ct)>=5)throw new LoiTaiKhoan(400,"Mỗi xác nhận nhận tối đa 5 ảnh bằng chứng.");
                db.Add(new BangChungGiaoNhan{XacNhanGiaoNhanId=receipt.Id,NguoiTaiLenId=actor,DuongDan=path,LoaiMime="image/jpeg",MaYeuCau=key,BamNoiDung=hash});await db.SaveChangesAsync(ct);
            }
            await tx.CommitAsync(ct);
        }
        catch{images.Xoa(path);throw;}
        return await DocAsync(id,actor,ct);
    }
    public async Task<string> DocBangChungAsync(Guid id,Guid actor,Guid file,CancellationToken ct)
    {
        var o=await CuaToiAsync(id,actor,ct);
        var path=await db.BangChungGiaoNhan.Where(x=>x.Id==file&&x.XacNhanGiaoNhan.GiaoDichId==o.GiaoDichId).Select(x=>x.DuongDan).SingleOrDefaultAsync(ct)??throw new LoiTaiKhoan(404,"Không tìm thấy bằng chứng.");
        var full=images.DuongDan(path);if(!File.Exists(full))throw new LoiTaiKhoan(404,"Không tìm thấy ảnh.");return full;
    }
    public async Task<DeNghiTraoDoiDto> DanhGiaAsync(Guid id,Guid actor,DanhGiaTraoDoiRequest r,CancellationToken ct)
    {
        Key(r.MaYeuCau);if(r.NoiDung.Trim().Length<5)throw new LoiTaiKhoan(400,"Nhận xét cần ít nhất 5 ký tự.");
        var selected=r.TieuChiIds.Distinct().Order().ToArray();
        if(await db.TieuChiDanhGia.CountAsync(x=>selected.Contains(x.Id),ct)!=selected.Length)throw new LoiTaiKhoan(400,"Tiêu chí đánh giá không hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(o,_,_)=await KhoaAsync(id,actor,ct);
        if(!await db.GiaoDich.AnyAsync(x=>x.Id==o.GiaoDichId&&x.TrangThai==TrangThaiGiaoDich.HoanTat,ct))throw new LoiTaiKhoan(409,"Chỉ đánh giá sau khi cả hai đã xác nhận hoàn tất.");
        var partner=actor==o.NguoiGuiId?o.NguoiNhanId:o.NguoiGuiId;
        var old=await db.DanhGia.SingleOrDefaultAsync(x=>x.GiaoDichId==o.GiaoDichId&&x.NguoiDanhGiaId==actor&&x.NguoiDuocDanhGiaId==partner,ct);
        if(old!=null)
        {
            var tags=await db.DanhGiaTieuChi.Where(x=>x.DanhGiaId==old.Id).Select(x=>x.TieuChiDanhGiaId).ToListAsync(ct);
            if(old.MaYeuCau!=r.MaYeuCau||old.SoSao!=r.SoSao||old.NoiDung!=r.NoiDung.Trim()||!tags.Order().SequenceEqual(selected))throw new LoiTaiKhoan(409,"Bạn đã đánh giá giao dịch này.");
        }
        else
        {
            var review=new DanhGia{GiaoDichId=o.GiaoDichId,NguoiDanhGiaId=actor,NguoiDuocDanhGiaId=partner,SoSao=r.SoSao,NoiDung=r.NoiDung.Trim(),MaYeuCau=r.MaYeuCau};db.Add(review);
            foreach(var tag in selected)db.Add(new DanhGiaTieuChi{DanhGiaId=review.Id,TieuChiDanhGiaId=tag});await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);
    }
}
