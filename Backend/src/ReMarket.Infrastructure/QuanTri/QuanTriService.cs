using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.QuanTri;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Catalog;
using ReMarket.Infrastructure.Commerce;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.QuanTri;

public sealed partial class QuanTriService(ReMarketDbContext db,KhoaTonKho stock,TinDangService catalog,KhoAnhSanPham images,TimeProvider clock)
{
    private DateTimeOffset Now=>clock.GetUtcNow();
    private static string Version(IThucThe x)=>Convert.ToBase64String(x.PhienBan);
    private static string Hash(object value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    private static void Page(int page){if(page is <1 or >100000)throw new LoiTaiKhoan(400,"Trang không hợp lệ.");}
    private static TrangQuanTri<T> PageDto<T>(List<T> rows,int total,int page)=>new(rows,total,page,(int)Math.Ceiling(total/12d));
    private async Task Admin(Guid actor,CancellationToken ct){if(!await db.NguoiDungVaiTro.AnyAsync(x=>x.NguoiDungId==actor&&x.VaiTro.Ma=="ADMIN"&&x.NguoiDung.TrangThai==TrangThaiNguoiDung.HoatDong,ct))throw new LoiTaiKhoan(403,"Chỉ ADMIN đang hoạt động được quản trị.");}
    private void Audit(Guid actor,string action,string type,Guid id,object data)=>db.Add(new NhatKyQuanTri{NguoiThucHienId=actor,HanhDong=action,LoaiDoiTuong=type,DoiTuongId=id,ChiTietJson=JsonSerializer.Serialize(data),NgayTao=Now});
    private void Notify(IEnumerable<Guid> users,Guid? gd,string title,string text,LoaiThongBao kind=LoaiThongBao.HeThong){foreach(var user in users.Distinct())db.Add(new ThongBao{NguoiDungId=user,GiaoDichId=gd,TieuDe=title,NoiDung=text,Loai=kind,NgayTao=Now});}
    private async Task<NguoiDung> LockUser(Guid id,CancellationToken ct)=>await db.NguoiDung.FromSqlInterpolated($"SELECT * FROM dbo.NguoiDung WITH (UPDLOCK,ROWLOCK) WHERE Id={id}").SingleOrDefaultAsync(ct)??throw new LoiTaiKhoan(404,"Không tìm thấy người dùng.");
    private async Task<List<NguoiDungQuanTriDto>> Users(IQueryable<NguoiDung> query,CancellationToken ct)
    {
        var users=await (from u in query join h in db.HoSoCaNhan on u.Id equals h.NguoiDungId select new{User=u,HoSo=h,
            Tin=db.TinDang.Count(t=>t.NguoiDangId==u.Id&&t.NgayXoa==null),GiaoDich=db.ThanhVienGiaoDich.Count(t=>t.NguoiDungId==u.Id),
            Khoa=db.KhoaTaiKhoan.Count(k=>k.NguoiDungId==u.Id),SoDanhGia=db.DanhGia.Count(r=>r.NguoiDuocDanhGiaId==u.Id),
            Diem=db.DanhGia.Where(r=>r.NguoiDuocDanhGiaId==u.Id).Select(r=>(double?)r.SoSao).Average()}).ToListAsync(ct);
        var ids=users.Select(x=>x.User.Id).ToArray();var roles=await db.NguoiDungVaiTro.Where(x=>ids.Contains(x.NguoiDungId)).Select(x=>new{x.NguoiDungId,x.VaiTro.Ma}).ToListAsync(ct);
        return users.Select(x=>new NguoiDungQuanTriDto(x.User.Id,x.HoSo.HoTen,x.User.Email,x.User.SoDienThoai,x.HoSo.TinhThanh,x.User.TrangThai.ToString(),Version(x.User),x.User.NgayTao,
            x.HoSo.DaXacMinhEmail,x.HoSo.DaXacMinhDienThoai,roles.Where(r=>r.NguoiDungId==x.User.Id).Select(r=>r.Ma).ToList(),x.Tin,x.GiaoDich,x.Khoa,x.Diem,x.SoDanhGia)).ToList();
    }
    public async Task<TrangQuanTri<NguoiDungQuanTriDto>> NguoiDungAsync(Guid actor,string? q,string? status,int page,CancellationToken ct)
    {
        await Admin(actor,ct);Page(page);var query=db.NguoiDung.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(q)){var text=q.Trim();query=query.Where(x=>x.Email.Contains(text)||(x.SoDienThoai!=null&&x.SoDienThoai.Contains(text))||db.HoSoCaNhan.Any(h=>h.NguoiDungId==x.Id&&EF.Functions.Collate(h.HoTen,"Latin1_General_100_CI_AI").Contains(text)));}
        if(!string.IsNullOrEmpty(status)){if(!Enum.TryParse<TrangThaiNguoiDung>(status,out var value)||!Enum.IsDefined(value))throw new LoiTaiKhoan(400,"Trạng thái người dùng không hợp lệ.");query=query.Where(x=>x.TrangThai==value);}
        var count=await query.CountAsync(ct);return PageDto(await Users(query.OrderByDescending(x=>x.NgayTao).ThenBy(x=>x.Id).Skip((page-1)*12).Take(12),ct),count,page);
    }
    public async Task<ChiTietNguoiDungQuanTriDto> NguoiDungAsync(Guid actor,Guid id,CancellationToken ct)
    {
        await Admin(actor,ct);var user=(await Users(db.NguoiDung.AsNoTracking().Where(x=>x.Id==id),ct)).SingleOrDefault()??throw new LoiTaiKhoan(404,"Không tìm thấy người dùng.");
        var locks=await (from l in db.KhoaTaiKhoan.AsNoTracking() join h in db.HoSoCaNhan on l.QuanTriVienId equals h.NguoiDungId where l.NguoiDungId==id orderby l.NgayTao descending select new KhoaQuanTriDto(l.Id,l.LyDo,l.NgayBatDau,l.NgayKetThuc,l.NgayMoKhoa,h.HoTen)).ToListAsync(ct);
        var log=await AuditRows("NguoiDung",id,ct);var protectedUser=user.VaiTro.Contains("ADMIN")||id==actor;
        return new(user,locks,log,!protectedUser&&user.TrangThai=="HoatDong",!protectedUser&&user.TrangThai=="DaKhoa");
    }
    private Task<List<MocQuanTriDto>> AuditRows(string type,Guid id,CancellationToken ct)=>(from l in db.NhatKyQuanTri.AsNoTracking() join h in db.HoSoCaNhan on l.NguoiThucHienId equals h.NguoiDungId where l.LoaiDoiTuong==type&&l.DoiTuongId==id orderby l.NgayTao descending select new MocQuanTriDto(l.NgayTao,l.HanhDong,h.HoTen,l.ChiTietJson)).Take(100).ToListAsync(ct);
    public async Task<ChiTietNguoiDungQuanTriDto> KhoaAsync(Guid actor,Guid id,KhoaNguoiDungRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);if(r.SoNgay is not(0 or 7 or 30)||r.LyDo.Trim().Length<5)throw new LoiTaiKhoan(400,"Chọn khóa 7/30 ngày hoặc vĩnh viễn và nhập lý do.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var u=await LockUser(id,ct);KhoaTonKho.PhienBan(u.PhienBan,r.PhienBan);
        if(id==actor||await db.NguoiDungVaiTro.AnyAsync(x=>x.NguoiDungId==id&&x.VaiTro.Ma=="ADMIN",ct))throw new LoiTaiKhoan(403,"Không tự khóa hoặc khóa tài khoản quản trị.");
        if(u.TrangThai!=TrangThaiNguoiDung.HoatDong)throw new LoiTaiKhoan(409,"Tài khoản không đang hoạt động.");
        u.TrangThai=TrangThaiNguoiDung.DaKhoa;db.Add(new KhoaTaiKhoan{NguoiDungId=id,QuanTriVienId=actor,LyDo=r.LyDo.Trim(),NgayBatDau=Now,NgayKetThuc=r.SoNgay==0?null:Now.AddDays(r.SoNgay),NgayTao=Now});
        await db.PhienDangNhap.Where(x=>x.NguoiDungId==id&&x.NgayThuHoi==null).ExecuteUpdateAsync(x=>x.SetProperty(p=>p.NgayThuHoi,Now),ct);
        Audit(actor,"Khóa tài khoản","NguoiDung",id,new{r.LyDo,r.SoNgay});Notify([id],null,"Tài khoản đã bị khóa",r.LyDo.Trim());await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await NguoiDungAsync(actor,id,ct);
    }
    public async Task<ChiTietNguoiDungQuanTriDto> MoKhoaAsync(Guid actor,Guid id,MoKhoaNguoiDungRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);if(!r.DaRaSoat||r.LyDo.Trim().Length<5)throw new LoiTaiKhoan(400,"Cần rà soát và ghi lý do mở khóa.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var u=await LockUser(id,ct);KhoaTonKho.PhienBan(u.PhienBan,r.PhienBan);
        if(id==actor||await db.NguoiDungVaiTro.AnyAsync(x=>x.NguoiDungId==id&&x.VaiTro.Ma=="ADMIN",ct))throw new LoiTaiKhoan(403,"Không thay trạng thái quản trị viên qua form này.");
        if(u.TrangThai!=TrangThaiNguoiDung.DaKhoa)throw new LoiTaiKhoan(409,"Tài khoản không đang khóa.");
        foreach(var l in await db.KhoaTaiKhoan.Where(x=>x.NguoiDungId==id&&x.NgayMoKhoa==null).ToListAsync(ct))l.NgayMoKhoa=Now;
        u.TrangThai=TrangThaiNguoiDung.HoatDong;Audit(actor,"Mở khóa tài khoản","NguoiDung",id,new{r.LyDo});Notify([id],null,"Tài khoản đã được mở khóa","Đăng nhập lại để tiếp tục. "+r.LyDo.Trim());
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await NguoiDungAsync(actor,id,ct);
    }
    public async Task<int> MoKhoaHetHanAsync(CancellationToken ct)
    {
        var ids=await db.KhoaTaiKhoan.AsNoTracking().Where(x=>x.NgayMoKhoa==null&&x.NgayKetThuc!=null&&x.NgayKetThuc<=Now).Select(x=>x.NguoiDungId).Distinct().OrderBy(x=>x).Take(50).ToListAsync(ct);var count=0;
        foreach(var id in ids){await using var tx=await db.Database.BeginTransactionAsync(ct);var u=await LockUser(id,ct);var locks=await db.KhoaTaiKhoan.Where(x=>x.NguoiDungId==id&&x.NgayMoKhoa==null).ToListAsync(ct);
            foreach(var l in locks.Where(x=>x.NgayKetThuc<=Now)){l.NgayMoKhoa=Now;Audit(l.QuanTriVienId,"Hết thời hạn khóa","NguoiDung",id,new{TuDong=true,l.Id});}
            if(u.TrangThai==TrangThaiNguoiDung.DaKhoa&&locks.Count>0&&locks.All(x=>x.NgayMoKhoa!=null)){u.TrangThai=TrangThaiNguoiDung.HoatDong;count++;Notify([id],null,"Đã hết thời hạn khóa","Bạn có thể đăng nhập lại. Lịch sử xử lý được giữ nguyên.");}
            await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);}
        return count;
    }
}
