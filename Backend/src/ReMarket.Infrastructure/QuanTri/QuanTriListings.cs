using Microsoft.EntityFrameworkCore;
using ReMarket.Application.QuanTri;
using ReMarket.Application.SanPham;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Commerce;

namespace ReMarket.Infrastructure.QuanTri;
public sealed partial class QuanTriService
{
    public async Task<TrangTinDto> TinAsync(Guid actor,string? q,string? status,int page,CancellationToken ct){await Admin(actor,ct);Page(page);return await catalog.QuanTriTimAsync(q,status,page,ct);}
    public async Task<ChiTietTinQuanTriDto> TinAsync(Guid actor,Guid id,CancellationToken ct)
    {
        await Admin(actor,ct);var row=await catalog.QuanTriDocAsync(id,ct);
        var log=await (from l in db.NhatKyKiemDuyet.AsNoTracking() join h in db.HoSoCaNhan on l.QuanTriVienId equals h.NguoiDungId where l.TinDangId==id orderby l.NgayTao descending select new MocQuanTriDto(l.NgayTao,l.TrangThaiSau.ToString(),h.HoTen,l.LyDo)).ToListAsync(ct);
        return new(row,log,row.TrangThai=="ChoDuyet"&&row.NguoiDangId!=actor,row.TrangThai=="DangHienThi"&&!await db.GiuChoSanPham.AnyAsync(x=>x.SanPhamId==row.SanPhamId&&!x.DaGiaiPhong,ct));
    }
    public async Task<ChiTietTinQuanTriDto> DuyetAsync(Guid actor,Guid id,DuyetTinRequest r,CancellationToken ct)
    {await Admin(actor,ct);await catalog.DuyetAsync(id,actor,r,ct);return await TinAsync(actor,id,ct);}
    public async Task<ChiTietTinQuanTriDto> AnTinAsync(Guid actor,Guid id,AnTinQuanTriRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);if(r.LyDo.Trim().Length<5)throw new LoiTaiKhoan(400,"Nêu rõ lý do ẩn tin.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(t,_)=await stock.TinAsync(id,ct);KhoaTonKho.PhienBan(t.PhienBan,r.PhienBan);
        if(t.NgayXoa!=null||t.TrangThai!=TrangThaiTinDang.DangHienThi||await db.GiuChoSanPham.AnyAsync(x=>x.SanPhamId==t.SanPhamId&&!x.DaGiaiPhong,ct))throw new LoiTaiKhoan(409,"Chỉ ẩn tin đang hiển thị và không bị giữ trong giao dịch.");
        var before=t.TrangThai;t.TrangThai=TrangThaiTinDang.DaAn;t.NgayCapNhat=Now;
        db.Add(new NhatKyKiemDuyet{TinDangId=id,QuanTriVienId=actor,TrangThaiTruoc=before,TrangThaiSau=t.TrangThai,LyDo=r.LyDo.Trim(),NgayTao=Now});Audit(actor,"Ẩn tin","TinDang",id,new{r.LyDo});Notify([t.NguoiDangId],null,"Tin đã bị ẩn",r.LyDo.Trim(),LoaiThongBao.TinDang);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await TinAsync(actor,id,ct);
    }
}
