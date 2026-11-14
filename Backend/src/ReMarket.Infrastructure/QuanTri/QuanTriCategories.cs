using Microsoft.EntityFrameworkCore;
using ReMarket.Application.QuanTri;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Commerce;

namespace ReMarket.Infrastructure.QuanTri;
public sealed partial class QuanTriService
{
    private static readonly string[] Icons=["recycle","smartphone","armchair","shirt","cooking-pot","book-open","bike"];
    public async Task<List<DanhMucQuanTriDto>> DanhMucAsync(Guid actor,CancellationToken ct)
    {
        await Admin(actor,ct);var rows=await db.DanhMuc.AsNoTracking().OrderBy(x=>x.ThuTu).ThenBy(x=>x.Ten).ToListAsync(ct);
        var counts=await db.TinDang.Where(x=>x.NgayXoa==null).GroupBy(x=>x.SanPham.DanhMucId).Select(x=>new{Id=x.Key,So=x.Count()}).ToListAsync(ct);
        return rows.Select(x=>{var ids=new HashSet<Guid>{x.Id};while(true){var next=rows.Where(c=>c.DanhMucChaId!=null&&ids.Contains(c.DanhMucChaId.Value)&&!ids.Contains(c.Id)).Select(c=>c.Id).ToArray();if(next.Length==0)break;ids.UnionWith(next);}
            return new DanhMucQuanTriDto(x.Id,x.Ten,x.Ma,x.DanhMucChaId,x.ThuTu,x.DangHienThi,x.BieuTuong,Version(x),counts.Where(c=>ids.Contains(c.Id)).Sum(c=>c.So),rows.Count(c=>c.DanhMucChaId==x.Id));}).ToList();
    }
    public async Task<DanhMucQuanTriDto> DanhMucAsync(Guid actor,Guid? id,LuuDanhMucQuanTriRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);if(r.Ten.Trim().Length<2||!Icons.Contains(r.BieuTuong))throw new LoiTaiKhoan(400,"Nhập tên và chọn biểu tượng hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource='remarket:categories',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=5000; IF @r<0 THROW 51020,'Categories busy',1;",ct);
        var all=await db.DanhMuc.ToListAsync(ct);var row=id==null?new DanhMuc{Ma="DM"+Guid.NewGuid().ToString("N")[..16]}:all.SingleOrDefault(x=>x.Id==id)??throw new LoiTaiKhoan(404,"Không tìm thấy danh mục.");
        if(id!=null){if(r.PhienBan==null)throw new LoiTaiKhoan(400,"Thiếu phiên bản.");KhoaTonKho.PhienBan(row.PhienBan,r.PhienBan);}
        var parent=r.DanhMucChaId;var seen=new HashSet<Guid>{row.Id};while(parent!=null){if(!seen.Add(parent.Value))throw new LoiTaiKhoan(400,"Danh mục cha tạo vòng lặp.");var p=all.SingleOrDefault(x=>x.Id==parent)??throw new LoiTaiKhoan(400,"Danh mục cha không tồn tại.");parent=p.DanhMucChaId;}
        if(all.Any(x=>x.Id!=row.Id&&x.DanhMucChaId==r.DanhMucChaId&&string.Equals(x.Ten,r.Ten.Trim(),StringComparison.OrdinalIgnoreCase)))throw new LoiTaiKhoan(409,"Tên danh mục trùng trong cùng nhóm.");
        row.Ten=r.Ten.Trim();row.DanhMucChaId=r.DanhMucChaId;row.ThuTu=r.ThuTu;row.DangHienThi=r.DangHienThi;row.BieuTuong=r.BieuTuong;if(id==null)db.Add(row);
        Audit(actor,id==null?"Thêm danh mục":"Sửa danh mục","DanhMuc",row.Id,new{row.Ten,row.DanhMucChaId,row.ThuTu,row.DangHienThi,row.BieuTuong});await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return(await DanhMucAsync(actor,ct)).Single(x=>x.Id==row.Id);
    }
    public async Task<BaoCaoQuanTriDto> BaoCaoAsync(Guid actor,DateOnly? from,DateOnly? to,CancellationToken ct)
    {
        await Admin(actor,ct);var today=DateOnly.FromDateTime(Now.ToOffset(TimeSpan.FromHours(7)).DateTime);var end=to??today;var start=from??end.AddDays(-29);
        if(start>end||end.DayNumber-start.DayNumber>365)throw new LoiTaiKhoan(400,"Khoảng ngày tối đa 366 ngày, ngày bắt đầu không sau ngày kết thúc.");
        if(end==DateOnly.MaxValue||start.DayNumber<end.DayNumber-start.DayNumber+1)throw new LoiTaiKhoan(400,"Khoảng ngày nằm ngoài phạm vi báo cáo được hỗ trợ.");
        var a=new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue),TimeSpan.FromHours(7));var b=new DateTimeOffset(end.AddDays(1).ToDateTime(TimeOnly.MinValue),TimeSpan.FromHours(7));var previous=a.AddDays(-(end.DayNumber-start.DayNumber+1));
        var trades=db.GiaoDich.AsNoTracking().Where(x=>x.NgayTao>=a&&x.NgayTao<b);var listings=db.TinDang.AsNoTracking().Where(x=>x.NgayTao>=a&&x.NgayTao<b);
        var tradeCounts=await trades.GroupBy(x=>x.Loai).Select(x=>new{x.Key,So=x.Count()}).ToListAsync(ct);
        var daily=await db.Database.SqlQuery<NgayQuanTriDb>($"SELECT CONVERT(date,SWITCHOFFSET(NgayTao,'+07:00')) AS Ngay, Loai, COUNT(*) AS So FROM dbo.GiaoDich WHERE NgayTao>={a} AND NgayTao<{b} GROUP BY CONVERT(date,SWITCHOFFSET(NgayTao,'+07:00')),Loai").ToListAsync(ct);
        var counts=await listings.GroupBy(x=>x.SanPham.DanhMucId).Select(x=>new{Id=x.Key,So=x.Count()}).ToListAsync(ct);var cats=await db.DanhMuc.AsNoTracking().ToListAsync(ct);
        Guid Root(Guid id){var visited=new HashSet<Guid>();while(visited.Add(id)){var p=cats.FirstOrDefault(x=>x.Id==id)?.DanhMucChaId;if(p==null)return id;id=p.Value;}return id;}
        var distribution=counts.GroupBy(x=>Root(x.Id)).Select(x=>new PhanBoQuanTriDto(x.Key,cats.Single(c=>c.Id==x.Key).Ten,x.Sum(c=>c.So))).OrderByDescending(x=>x.SoTin).ToList();
        var complaints=db.KhieuNai.AsNoTracking().Where(x=>x.NgayTao>=a&&x.NgayTao<b);
        var chart=Enumerable.Range(0,end.DayNumber-start.DayNumber+1).Select(i=>{var date=start.AddDays(i);return new CotThongKeDto(date,daily.Where(x=>DateOnly.FromDateTime(x.Ngay)==date&&x.Loai=="MuaBan").Sum(x=>x.So),daily.Where(x=>DateOnly.FromDateTime(x.Ngay)==date&&x.Loai!="MuaBan").Sum(x=>x.So));}).ToList();
        return new(start,end,Now,await db.NguoiDung.CountAsync(ct),await db.NguoiDung.CountAsync(x=>x.NgayTao>=a&&x.NgayTao<b,ct),await listings.CountAsync(ct),await db.TinDang.CountAsync(x=>x.NgayXoa==null&&x.TrangThai==Domain.Enums.TrangThaiTinDang.ChoDuyet,ct),
            await listings.CountAsync(x=>x.NguoiDuyetId!=null&&x.TrangThai!=Domain.Enums.TrangThaiTinDang.TuChoi,ct),await listings.CountAsync(x=>x.TrangThai==Domain.Enums.TrangThaiTinDang.TuChoi,ct),
            tradeCounts.Where(x=>x.Key==Domain.Enums.LoaiGiaoDich.MuaBan).Sum(x=>x.So),tradeCounts.Where(x=>x.Key!=Domain.Enums.LoaiGiaoDich.MuaBan).Sum(x=>x.So),tradeCounts.Where(x=>x.Key==Domain.Enums.LoaiGiaoDich.SmartSwap).Sum(x=>x.So),
            await complaints.CountAsync(x=>x.TrangThai==Domain.Enums.TrangThaiKhieuNai.Moi,ct),await complaints.CountAsync(x=>x.TrangThai==Domain.Enums.TrangThaiKhieuNai.DangXuLy||x.TrangThai==Domain.Enums.TrangThaiKhieuNai.ChoThucHien,ct),await complaints.CountAsync(x=>x.TrangThai==Domain.Enums.TrangThaiKhieuNai.DaGiaiQuyet,ct),
            await db.GiaoDich.CountAsync(x=>x.NgayTao>=previous&&x.NgayTao<a,ct),chart,distribution);
    }
}
public sealed class NgayQuanTriDb { public DateTime Ngay {get;set;} public string Loai {get;set;}=""; public int So {get;set;} }
