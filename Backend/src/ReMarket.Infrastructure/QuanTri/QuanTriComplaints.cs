using Microsoft.EntityFrameworkCore;
using ReMarket.Application.QuanTri;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Commerce;

namespace ReMarket.Infrastructure.QuanTri;
public sealed partial class QuanTriService
{
    public async Task<ChiTietKhieuNaiQuanTriDto> TaoKhieuNaiAsync(Guid actor,Guid gd,TaoKhieuNaiQuanTriRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);
        if(r.MaYeuCau==Guid.Empty||!r.DaDoiChieu||r.NguoiGuiId==r.NguoiBiKhieuNaiId||r.TieuDe.Trim().Length<5||r.NoiDung.Trim().Length<10)throw new LoiTaiKhoan(400,"Đối chiếu nội dung, chọn hai thành viên khác nhau và nêu rõ phản ánh.");
        var hash=Hash(new{Actor=actor,GiaoDichId=gd,r.NguoiGuiId,r.NguoiBiKhieuNaiId,TieuDe=r.TieuDe.Trim(),NoiDung=r.NoiDung.Trim()});
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var products=await ProductIds(gd,ct);await stock.NhieuTinAsync(await db.TinDang.Where(x=>products.Contains(x.SanPhamId)).Select(x=>x.Id).ToListAsync(ct),ct);
        var v=await db.VongSmartSwap.FromSqlInterpolated($"SELECT * FROM dbo.VongSmartSwap WITH (UPDLOCK,ROWLOCK) WHERE GiaoDichId={gd}").SingleOrDefaultAsync(ct);
        var g=await db.GiaoDich.FromSqlInterpolated($"SELECT * FROM dbo.GiaoDich WITH (UPDLOCK,ROWLOCK) WHERE Id={gd}").SingleOrDefaultAsync(ct)??throw new LoiTaiKhoan(404,"Không tìm thấy giao dịch.");
        var members=await Participants(gd,ct);if(!members.Contains(r.NguoiGuiId)||!members.Contains(r.NguoiBiKhieuNaiId))throw new LoiTaiKhoan(400,"Chỉ chọn thành viên thuộc giao dịch.");
        var old=await db.KhieuNai.SingleOrDefaultAsync(x=>x.NguoiGuiId==r.NguoiGuiId&&x.MaYeuCau==r.MaYeuCau,ct);
        if(old!=null){if(old.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã phản ánh đã được dùng cho nội dung khác.");await tx.CommitAsync(ct);return await KhieuNaiAsync(actor,old.Id,ct);}
        if(g.TrangThai is TrangThaiGiaoDich.DaHuy or TrangThaiGiaoDich.ChoDongY)throw new LoiTaiKhoan(409,"Chỉ ghi nhận phản ánh cho giao dịch đã chính thức.");
        var k=new KhieuNai{Ma="KN"+Guid.NewGuid().ToString("N")[..20],GiaoDichId=gd,NguoiGuiId=r.NguoiGuiId,NguoiBiKhieuNaiId=r.NguoiBiKhieuNaiId,TieuDe=r.TieuDe.Trim(),NoiDung=r.NoiDung.Trim(),TrangThai=TrangThaiKhieuNai.DangXuLy,HanPhanHoi=Now.AddDays(3),MaYeuCau=r.MaYeuCau,BamYeuCau=hash,NgayTao=Now};db.Add(k);
        // The administrator records a reported issue; the audit explicitly identifies the real actor.
        db.Add(new PhanHoiKhieuNai{KhieuNaiId=k.Id,NguoiGuiId=actor,NoiDung="Quản trị ghi nhận phản ánh sau đối chiếu: "+r.NoiDung.Trim(),NgayTao=Now});
        if(g.TrangThai==TrangThaiGiaoDich.DangThucHien&&g.Loai!=LoaiGiaoDich.MuaBan){g.TrangThai=TrangThaiGiaoDich.TranhChap;if(v!=null)v.TrangThai=TrangThaiVong.TamDung;}
        Audit(actor,"Ghi nhận phản ánh","KhieuNai",k.Id,new{GiaoDichId=gd,r.NguoiGuiId,r.NguoiBiKhieuNaiId,r.NoiDung});Notify(members,gd,"Quản trị tiếp nhận phản ánh "+k.Ma,k.TieuDe,LoaiThongBao.KhieuNai);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await KhieuNaiAsync(actor,k.Id,ct);
    }
    private Task<List<KhieuNaiQuanTriDto>> Complaints(IQueryable<KhieuNai> query,CancellationToken ct)=>(from k in query join a in db.HoSoCaNhan on k.NguoiGuiId equals a.NguoiDungId join b in db.HoSoCaNhan on k.NguoiBiKhieuNaiId equals b.NguoiDungId orderby k.HanPhanHoi,k.NgayTao,k.Id
        select new KhieuNaiQuanTriDto(k.Id,k.Ma,k.GiaoDichId,k.GiaoDich.Ma,k.GiaoDich.Loai.ToString(),k.NguoiGuiId,a.HoTen,k.NguoiBiKhieuNaiId,b.HoTen,k.TieuDe,k.NoiDung,k.TrangThai.ToString(),k.NgayTao,k.HanPhanHoi,Convert.ToBase64String(k.PhienBan))).ToListAsync(ct);
    public async Task<TrangQuanTri<KhieuNaiQuanTriDto>> KhieuNaiAsync(Guid actor,string? q,string? status,int page,CancellationToken ct)
    {
        await Admin(actor,ct);Page(page);var query=db.KhieuNai.AsNoTracking();if(!string.IsNullOrWhiteSpace(q)){var text=q.Trim();query=query.Where(x=>x.Ma.Contains(text)||x.GiaoDich.Ma.Contains(text)||x.TieuDe.Contains(text));}
        if(!string.IsNullOrEmpty(status)){if(!Enum.TryParse<TrangThaiKhieuNai>(status,out var value)||!Enum.IsDefined(value))throw new LoiTaiKhoan(400,"Trạng thái khiếu nại không hợp lệ.");query=query.Where(x=>x.TrangThai==value);}
        var count=await query.CountAsync(ct);var ids=await query.OrderBy(x=>x.HanPhanHoi).ThenBy(x=>x.NgayTao).ThenBy(x=>x.Id).Skip((page-1)*12).Take(12).Select(x=>x.Id).ToListAsync(ct);
        return PageDto(await Complaints(db.KhieuNai.AsNoTracking().Where(x=>ids.Contains(x.Id)),ct),count,page);
    }
    public async Task<ChiTietKhieuNaiQuanTriDto> KhieuNaiAsync(Guid actor,Guid id,CancellationToken ct)
    {
        await Admin(actor,ct);var k=(await Complaints(db.KhieuNai.AsNoTracking().Where(x=>x.Id==id),ct)).SingleOrDefault()??throw new LoiTaiKhoan(404,"Không tìm thấy khiếu nại.");
        var responses=await (from p in db.PhanHoiKhieuNai.AsNoTracking() join h in db.HoSoCaNhan on p.NguoiGuiId equals h.NguoiDungId where p.KhieuNaiId==id orderby p.NgayTao select new MocQuanTriDto(p.NgayTao,"Phản hồi",h.HoTen,p.NoiDung)).ToListAsync(ct);
        var decisions=await db.QuyetDinhXuLy.AsNoTracking().Where(x=>x.KhieuNaiId==id).OrderByDescending(x=>x.NgayTao).Select(x=>new QuyetDinhQuanTriDto(x.Id,x.KetLuan,x.BienPhap,x.CanCu,x.ThongBao,x.HuongXuLy,x.DaXacMinhThucHien,x.NgayTao)).ToListAsync(ct);
        return new(k,await Evidence(k.GiaoDichId,ct),responses,decisions);
    }
    private async Task<(KhieuNai K,GiaoDich G,Dictionary<Guid,TinDang> Tin,VongSmartSwap? V)> LockComplaint(Guid id,CancellationToken ct)
    {
        var gd=await db.KhieuNai.AsNoTracking().Where(x=>x.Id==id).Select(x=>(Guid?)x.GiaoDichId).SingleOrDefaultAsync(ct)??throw new LoiTaiKhoan(404,"Không tìm thấy khiếu nại.");
        var products=await ProductIds(gd,ct);var ids=await db.TinDang.Where(x=>products.Contains(x.SanPhamId)).Select(x=>x.Id).ToListAsync(ct);var tins=await stock.NhieuTinAsync(ids,ct);
        var v=await db.VongSmartSwap.FromSqlInterpolated($"SELECT * FROM dbo.VongSmartSwap WITH (UPDLOCK,ROWLOCK) WHERE GiaoDichId={gd}").SingleOrDefaultAsync(ct);
        var g=await db.GiaoDich.FromSqlInterpolated($"SELECT * FROM dbo.GiaoDich WITH (UPDLOCK,ROWLOCK) WHERE Id={gd}").SingleAsync(ct);
        var k=await db.KhieuNai.FromSqlInterpolated($"SELECT * FROM dbo.KhieuNai WITH (UPDLOCK,ROWLOCK) WHERE Id={id}").SingleAsync(ct);return(k,g,tins,v);
    }
    private Task<List<Guid>> Participants(Guid gd,CancellationToken ct)=>db.ThanhVienGiaoDich.Where(x=>x.GiaoDichId==gd).Select(x=>x.NguoiDungId).ToListAsync(ct);
    public async Task<ChiTietKhieuNaiQuanTriDto> TiepNhanAsync(Guid actor,Guid id,TiepNhanKhieuNaiRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);if(r.NoiDung.Trim().Length<5)throw new LoiTaiKhoan(400,"Nhập nội dung tiếp nhận/yêu cầu bổ sung.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(k,_,_,_)=await LockComplaint(id,ct);KhoaTonKho.PhienBan(k.PhienBan,r.PhienBan);
        if(k.TrangThai is not(TrangThaiKhieuNai.Moi or TrangThaiKhieuNai.DangXuLy))throw new LoiTaiKhoan(409,"Khiếu nại không ở giai đoạn tiếp nhận.");
        k.TrangThai=TrangThaiKhieuNai.DangXuLy;db.Add(new PhanHoiKhieuNai{KhieuNaiId=id,NguoiGuiId=actor,NoiDung=r.NoiDung.Trim(),NgayTao=Now});Audit(actor,"Tiếp nhận/phản hồi","KhieuNai",id,new{r.NoiDung});
        Notify(await Participants(k.GiaoDichId,ct),k.GiaoDichId,"Cập nhật khiếu nại "+k.Ma,r.NoiDung.Trim(),LoaiThongBao.KhieuNai);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await KhieuNaiAsync(actor,id,ct);
    }
    public async Task<ChiTietKhieuNaiQuanTriDto> QuyetDinhAsync(Guid actor,Guid id,QuyetDinhQuanTriRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);if(r.MaYeuCau==Guid.Empty||!r.GuiThongBao||r.CanCu.Trim().Length<10||r.KetLuan.Trim().Length<5||r.BienPhap.Trim().Length<5||r.ThongBao.Trim().Length<10||r.HuongXuLy is not("TiepTuc" or "KetThuc" or "GhiNhan"))throw new LoiTaiKhoan(400,"Cần căn cứ, kết luận, biện pháp, thông báo và hướng xử lý rõ ràng.");
        var hash=Hash(new{Actor=actor,CanCu=r.CanCu.Trim(),KetLuan=r.KetLuan.Trim(),BienPhap=r.BienPhap.Trim(),ThongBao=r.ThongBao.Trim(),r.HuongXuLy});
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(k,g,_,_)=await LockComplaint(id,ct);
        var old=await db.QuyetDinhXuLy.SingleOrDefaultAsync(x=>x.KhieuNaiId==id&&x.MaYeuCau==r.MaYeuCau,ct);
        if(old!=null){if(old.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã quyết định đã dùng cho nội dung khác.");await tx.CommitAsync(ct);return await KhieuNaiAsync(actor,id,ct);}
        KhoaTonKho.PhienBan(k.PhienBan,r.PhienBan);
        if(k.TrangThai!=TrangThaiKhieuNai.DangXuLy)throw new LoiTaiKhoan(409,"Tiếp nhận khiếu nại trước khi quyết định.");
        if(g.Loai==LoaiGiaoDich.MuaBan&&r.HuongXuLy!="GhiNhan")throw new LoiTaiKhoan(400,"Tranh chấp mua bán ghi nhận xử lý thủ công; không tự sửa thanh toán/đơn hàng qua quyết định này.");
        var decision=new QuyetDinhXuLy{KhieuNaiId=id,QuanTriVienId=actor,CanCu=r.CanCu.Trim(),KetLuan=r.KetLuan.Trim(),BienPhap=r.BienPhap.Trim(),ThongBao=r.ThongBao.Trim(),HuongXuLy=r.HuongXuLy,MaYeuCau=r.MaYeuCau,BamYeuCau=hash,NgayTao=Now};db.Add(decision);k.TrangThai=TrangThaiKhieuNai.ChoThucHien;
        db.Add(new PhanHoiKhieuNai{KhieuNaiId=id,NguoiGuiId=actor,NoiDung="Quyết định: "+r.KetLuan.Trim()+". "+r.ThongBao.Trim(),NgayTao=Now});Audit(actor,"Quyết định tranh chấp","KhieuNai",id,new{decision.Id,decision.KetLuan,decision.BienPhap,decision.CanCu,decision.HuongXuLy});
        Notify(await Participants(k.GiaoDichId,ct),k.GiaoDichId,"Quyết định khiếu nại "+k.Ma,r.ThongBao.Trim(),LoaiThongBao.KhieuNai);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await KhieuNaiAsync(actor,id,ct);
    }
    public async Task<ChiTietKhieuNaiQuanTriDto> XacMinhAsync(Guid actor,Guid id,XacMinhQuyetDinhRequest r,CancellationToken ct)
    {
        await Admin(actor,ct);if(!r.DaXacMinh||r.NoiDung.Trim().Length<10)throw new LoiTaiKhoan(400,"Chỉ đóng khi đã kiểm tra các bên thực hiện và ghi căn cứ xác minh.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(k,g,tins,v)=await LockComplaint(id,ct);var p=await db.QuyetDinhXuLy.SingleOrDefaultAsync(x=>x.Id==r.QuyetDinhId&&x.KhieuNaiId==id,ct)??throw new LoiTaiKhoan(404,"Không tìm thấy quyết định của khiếu nại.");
        if(p.DaXacMinhThucHien){await tx.CommitAsync(ct);return await KhieuNaiAsync(actor,id,ct);}
        KhoaTonKho.PhienBan(k.PhienBan,r.PhienBan);if(k.TrangThai!=TrangThaiKhieuNai.ChoThucHien)throw new LoiTaiKhoan(409,"Khiếu nại chưa có quyết định chờ thực hiện.");
        p.DaXacMinhThucHien=true;
        await db.SaveChangesAsync(ct);
        // A temporary hold is not a final resolution: keep the complaint open for a later decision.
        var continueHandling=g.Loai!=LoaiGiaoDich.MuaBan&&p.HuongXuLy=="GhiNhan";
        k.TrangThai=continueHandling?TrangThaiKhieuNai.DangXuLy:TrangThaiKhieuNai.DaGiaiQuyet;
        // Persist the resolved complaint first, within this transaction, before SQL workflow guards run.
        await db.SaveChangesAsync(ct);
        var allClosed=!await db.KhieuNai.AnyAsync(x=>x.GiaoDichId==g.Id&&x.TrangThai!=TrangThaiKhieuNai.DaGiaiQuyet&&x.TrangThai!=TrangThaiKhieuNai.TuChoi,ct);
        var directions=await db.QuyetDinhXuLy.Where(x=>x.KhieuNai.GiaoDichId==g.Id&&x.DaXacMinhThucHien&&
            (x.HuongXuLy!="GhiNhan"||!db.QuyetDinhXuLy.Any(later=>later.KhieuNaiId==x.KhieuNaiId&&later.DaXacMinhThucHien&&later.HuongXuLy!="GhiNhan"))).Select(x=>x.HuongXuLy).ToListAsync(ct);
        var direction=directions.Contains("KetThuc")?"KetThuc":directions.Contains("GhiNhan")?"GhiNhan":"TiepTuc";
        if(allClosed&&g.TrangThai==TrangThaiGiaoDich.TranhChap&&g.Loai!=LoaiGiaoDich.MuaBan&&direction!="GhiNhan")
        {
            if(direction=="TiepTuc")
            {
                var agreement=await db.PhienBanThoaThuan.Where(x=>x.GiaoDichId==g.Id).OrderByDescending(x=>x.SoPhienBan).FirstOrDefaultAsync(ct);
                var expected=g.Loai==LoaiGiaoDich.SmartSwap?3:2;
                if(agreement?.TrangThai!=TrangThaiThoaThuan.DaChot||await db.DongYThoaThuan.CountAsync(x=>x.PhienBanThoaThuanId==agreement.Id&&x.QuyetDinh==QuyetDinhDongY.DongY,ct)!=expected||await db.GiuChoSanPham.CountAsync(x=>x.GiaoDichId==g.Id&&!x.DaGiaiPhong,ct)!=expected)throw new LoiTaiKhoan(409,"Không đủ thỏa thuận/giữ sản phẩm để tiếp tục. Chọn phương án xử lý phù hợp.");
                if(v!=null){v.TrangThai=await db.VanChuyenMoPhong.AnyAsync(x=>x.ChangTraoDoi!.VongSmartSwapId==v.Id,ct)?TrangThaiVong.DangBanGiao:TrangThaiVong.SanSangBanGiao;await db.SaveChangesAsync(ct);}
                g.TrangThai=TrangThaiGiaoDich.DangThucHien;
            }
            else
            {
                g.TrangThai=TrangThaiGiaoDich.DaHuy;if(v!=null){v.TrangThai=TrangThaiVong.DaHuy;v.MaKetThuc="DaHuy";v.LyDoKetThuc="Kết thúc sau xác minh quyết định tranh chấp.";}
                foreach(var hold in await db.GiuChoSanPham.Where(x=>x.GiaoDichId==g.Id&&!x.DaGiaiPhong).ToListAsync(ct))hold.DaGiaiPhong=true;
                foreach(var tin in tins.Values.Where(x=>x.TrangThai==TrangThaiTinDang.DangGiuCho)){tin.TrangThai=TrangThaiTinDang.DaAn;tin.NgayCapNhat=Now;}
            }
        }
        db.Add(new PhanHoiKhieuNai{KhieuNaiId=id,NguoiGuiId=actor,NoiDung="Đã xác minh thực hiện: "+r.NoiDung.Trim(),NgayTao=Now});
        Audit(actor,"Xác minh quyết định","KhieuNai",id,new{r.QuyetDinhId,r.NoiDung,p.HuongXuLy});Notify(await Participants(g.Id,ct),g.Id,"Đã xác minh khiếu nại "+k.Ma,r.NoiDung.Trim(),LoaiThongBao.KhieuNai);
        // Never insert/update XacNhanGiaoNhan, shipment receipts or compensation acknowledgements here.
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await KhieuNaiAsync(actor,id,ct);
    }
}
