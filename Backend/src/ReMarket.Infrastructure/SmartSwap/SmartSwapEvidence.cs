using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed partial class VongSmartSwapService
{
    public async Task<GiaoNhanSmartSwapDto> BangChungAsync(Guid id,Guid actor,Guid key,Stream input,CancellationToken ct)
    {
        Key(key);await CuaToiAsync(id,actor,ct);using var buffer=new MemoryStream();var chunk=new byte[16384];
        while(true){var n=await input.ReadAsync(chunk,ct);if(n==0)break;if(buffer.Length+n>5*1024*1024)throw new LoiTaiKhoan(400,"Ảnh tối đa 5 MB.");await buffer.WriteAsync(chunk.AsMemory(0,n),ct);}
        var hash=Convert.ToHexString(SHA256.HashData(buffer.ToArray()));buffer.Position=0;var path=await images.LuuAsync(buffer,ct);
        try
        {
            await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,_)=await KhoaAsync(id,actor,ct);
            var receipt=await db.XacNhanGiaoNhan.SingleOrDefaultAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.NguoiXacNhanId==actor&&x.Loai==LoaiXacNhan.DaNhan,ct)
                ??throw new LoiTaiKhoan(409,"Bạn cần xác nhận nhận đúng hàng trước khi lưu bằng chứng.");
            var old=await db.BangChungGiaoNhan.SingleOrDefaultAsync(x=>x.XacNhanGiaoNhanId==receipt.Id&&x.MaYeuCau==key,ct);
            if(old!=null){if(old.BamNoiDung!=hash)throw new LoiTaiKhoan(409,"Mã ảnh đã dùng cho nội dung khác.");images.Xoa(path);}
            else
            {
                if(await db.BangChungGiaoNhan.CountAsync(x=>x.XacNhanGiaoNhanId==receipt.Id,ct)>=5)throw new LoiTaiKhoan(400,"Mỗi người tối đa 5 ảnh bằng chứng.");
                db.Add(new BangChungGiaoNhan{XacNhanGiaoNhanId=receipt.Id,NguoiTaiLenId=actor,DuongDan=path,LoaiMime="image/jpeg",MaYeuCau=key,BamNoiDung=hash});await db.SaveChangesAsync(ct);
            }
            await tx.CommitAsync(ct);
        }catch{images.Xoa(path);throw;}
        return await GiaoNhanAsync(id,actor,ct);
    }
    public async Task<string> AnhBangChungAsync(Guid id,Guid actor,Guid file,CancellationToken ct)
    {
        var v=await CuaToiAsync(id,actor,ct);var path=await db.BangChungGiaoNhan.Where(x=>x.Id==file&&x.XacNhanGiaoNhan.GiaoDichId==v.GiaoDichId).Select(x=>x.DuongDan).SingleOrDefaultAsync(ct)
            ??throw new LoiTaiKhoan(404,"Không tìm thấy bằng chứng.");var full=images.DuongDan(path);if(!File.Exists(full))throw new LoiTaiKhoan(404,"Không tìm thấy ảnh.");return full;
    }
}
