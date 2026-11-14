using Microsoft.EntityFrameworkCore;
using ReMarket.Application.TaiKhoan;
using ReMarket.Application.TraoDoi;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Commerce;

namespace ReMarket.Infrastructure.Exchange;

public sealed partial class TraoDoiService
{
    public async Task<DeNghiTraoDoiDto> PhanHoiAsync(Guid id,Guid actor,PhanHoiDeNghiRequest r,CancellationToken ct)
    {
        if(!r.DongY)return await KetThucAsync(id,actor,r.PhienBan,r.LyDo??"Từ chối đề nghị trao đổi",false,true,ct);
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var(o,gui,nhan)=await KhoaAsync(id,actor,ct);var p=await HienTaiAsync(o.GiaoDichId,ct);
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==o.GiaoDichId,ct);
        if(p.NguoiTaoId==actor)throw new LoiTaiKhoan(403,"Bạn không thể tự chấp nhận đề xuất của mình.");
        if(o.TrangThai==TrangThaiDeNghi.DaChapNhan){await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);}
        KhoaTonKho.PhienBan(o.PhienBan,r.PhienBan);
        if(!Pending(o)||o.NgayHetHan<=Now||gd.TrangThai!=TrangThaiGiaoDich.ChoDongY)throw new LoiTaiKhoan(409,"Đề nghị đã xử lý, tranh chấp hoặc hết hạn.");
        await KhaDungAsync(gui,o.GiaoDichId,ct);await KhaDungAsync(nhan,o.GiaoDichId,ct);
        var money=await db.TienBu.SingleOrDefaultAsync(x=>x.PhienBanThoaThuanId==p.Id,ct);
        if(money!=null)await BuTienAsync(gui,nhan,money.SoTien,money.NguoiTraId,ct);
        var consent=await db.DongYThoaThuan.SingleAsync(x=>x.PhienBanThoaThuanId==p.Id&&x.NguoiDungId==actor,ct);
        consent.QuyetDinh=QuyetDinhDongY.DongY;consent.NgayPhanHoi=Now;
        await db.SaveChangesAsync(ct); // Both consents must exist before closing the version.
        p.TrangThai=TrangThaiThoaThuan.DaChot;await db.SaveChangesAsync(ct);
        foreach(var tin in new[]{gui,nhan})
        {
            if(!await db.GiuChoSanPham.AnyAsync(x=>x.SanPhamId==tin.SanPhamId&&!x.DaGiaiPhong&&x.GiaoDichId==o.GiaoDichId,ct))
                db.Add(new GiuChoSanPham{GiaoDichId=o.GiaoDichId,SanPhamId=tin.SanPhamId,NgayHetHan=Now.AddHours(48)});
            tin.TrangThai=TrangThaiTinDang.DangGiuCho;tin.NgayCapNhat=Now;
        }
        o.TrangThai=TrangThaiDeNghi.DaChapNhan;
        gd.TrangThai=TrangThaiGiaoDich.DangThucHien;
        Notify(o,"Đã chấp nhận trao đổi","Hai món đã được giữ. Chỉ bàn giao theo thỏa thuận hiện tại.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);
    }
    public async Task<DeNghiTraoDoiDto> DeXuatLaiAsync(Guid id,Guid actor,DoiDeNghiRequest r,CancellationToken ct)
    {
        if(r.LoiNhan.Trim().Length<5||r.ThoiGianHen is {} time&&time<=Now)throw new LoiTaiKhoan(400,"Nhập lời nhắn và thời gian hẹn hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var(o,gui,nhan)=await KhoaAsync(id,actor,ct);KhoaTonKho.PhienBan(o.PhienBan,r.PhienBan);
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==o.GiaoDichId,ct);
        if(gd.TrangThai is not(TrangThaiGiaoDich.ChoDongY or TrangThaiGiaoDich.DangThucHien))throw new LoiTaiKhoan(409,"Giao dịch đang tranh chấp hoặc đã kết thúc.");
        if((!Pending(o)&&o.TrangThai!=TrangThaiDeNghi.DaChapNhan)||o.NgayHetHan<=Now&&Pending(o)||await db.XacNhanGiaoNhan.AnyAsync(x=>x.GiaoDichId==o.GiaoDichId,ct))
            throw new LoiTaiKhoan(409,"Không đề xuất lại khi đã hết hạn, kết thúc hoặc đã bàn giao.");
        await KhaDungAsync(gui,o.GiaoDichId,ct);await KhaDungAsync(nhan,o.GiaoDichId,ct);
        await BuTienAsync(gui,nhan,r.SoTienBu,r.NguoiTraId??o.NguoiGuiId,ct);
        var p=await HienTaiAsync(o.GiaoDichId,ct);p.TrangThai=TrangThaiThoaThuan.DaThayThe;
        gd.TrangThai=TrangThaiGiaoDich.ChoDongY;
        await db.SaveChangesAsync(ct); // Close old immutable terms before creating the replacement.
        PhienMoi(o,p.SoPhienBan+1,actor,r.SoTienBu,r.NguoiTraId??o.NguoiGuiId,r.LoiNhan,r.DiaDiemHen,r.ThoiGianHen);
        o.TrangThai=TrangThaiDeNghi.DeXuatLai;o.NgayHetHan=Now.AddHours(24);
        Notify(o,"Đề xuất trao đổi mới","Tiền bù/lịch hẹn mới cần đối tác đồng ý. Bàn giao đang khóa.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);
    }
    public async Task<DeNghiTraoDoiDto> KetThucAsync(Guid id,Guid? actor,string? version,string reason,bool expired,bool reject,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var(o,gui,nhan)=await KhoaAsync(id,actor,ct);
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==o.GiaoDichId,ct);
        if(o.TrangThai is TrangThaiDeNghi.DaHuy or TrangThaiDeNghi.TuChoi or TrangThaiDeNghi.HetHan)
        {await tx.CommitAsync(ct);return await DocAsync(id,actor??o.NguoiGuiId,ct,false);}
        if(gd.TrangThai is not(TrangThaiGiaoDich.ChoDongY or TrangThaiGiaoDich.DangThucHien))
        {if(expired){await tx.CommitAsync(ct);return await DocAsync(id,actor??o.NguoiGuiId,ct,false);}throw new LoiTaiKhoan(409,"Giao dịch đang tranh chấp hoặc đã kết thúc.");}
        if(o.TrangThai is TrangThaiDeNghi.DaHuy or TrangThaiDeNghi.TuChoi or TrangThaiDeNghi.HetHan||expired&&(!Pending(o)||o.NgayHetHan>Now))
        {await tx.CommitAsync(ct);return await DocAsync(id,actor??o.NguoiGuiId,ct,false);}
        if(!Pending(o)&&o.TrangThai!=TrangThaiDeNghi.DaChapNhan||await db.XacNhanGiaoNhan.AnyAsync(x=>x.GiaoDichId==o.GiaoDichId,ct))throw new LoiTaiKhoan(409,"Đã bàn giao hoặc kết thúc; không hủy hay từ chối được.");
        if(!expired)
        {
            KhoaTonKho.PhienBan(o.PhienBan,version??"");
            if(reason.Trim().Length<5)throw new LoiTaiKhoan(400,"Cần lý do ít nhất 5 ký tự.");
            if(reject){var current=await HienTaiAsync(o.GiaoDichId,ct);if(current.NguoiTaoId==actor||!Pending(o))throw new LoiTaiKhoan(403,"Chỉ đối tác nhận đề xuất hiện tại được từ chối.");}
        }
        o.TrangThai=expired?TrangThaiDeNghi.HetHan:reject?TrangThaiDeNghi.TuChoi:TrangThaiDeNghi.DaHuy;o.LyDoKetThuc=reason.Trim();
        gd.TrangThai=TrangThaiGiaoDich.DaHuy;
        foreach(var tin in new[]{gui,nhan})
        {
            var held=await db.GiuChoSanPham.Where(x=>x.SanPhamId==tin.SanPhamId&&x.GiaoDichId==o.GiaoDichId&&!x.DaGiaiPhong).ToListAsync(ct);
            foreach(var hold in held)hold.DaGiaiPhong=true;
            if(held.Count>0&&tin.NgayXoa==null&&tin.TrangThai==TrangThaiTinDang.DangGiuCho){tin.TrangThai=TrangThaiTinDang.DangHienThi;tin.NgayCapNhat=Now;}
        }
        Notify(o,"Đề nghị đã kết thúc",reason.Trim());await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(id,actor??o.NguoiGuiId,ct,false);
    }
    public async Task HetHanAsync(CancellationToken ct)
    {
        var ids=await db.DeNghiTraoDoi.AsNoTracking().Where(x=>x.GiaoDich.TrangThai==TrangThaiGiaoDich.ChoDongY&&(x.TrangThai==TrangThaiDeNghi.ChoPhanHoi||x.TrangThai==TrangThaiDeNghi.DeXuatLai)&&x.NgayHetHan<=Now).OrderBy(x=>x.NgayHetHan).Take(20).Select(x=>x.Id).ToListAsync(ct);
        foreach(var id in ids)await KetThucAsync(id,null,null,"Hết hạn phản hồi đề nghị",true,false,ct);
    }
    public async Task<DeNghiTraoDoiDto> GiaoNhanAsync(Guid id,Guid actor,GiaoNhanTraoDoiRequest r,CancellationToken ct)
    {
        if(!r.DaKiemTra||!r.DaGiaoMonCuaToi||!r.DaNhanDu||!r.DaXacNhanTienBu)throw new LoiTaiKhoan(400,"Chỉ xác nhận khi đã giao, nhận, kiểm tra món đồ và xác nhận đủ tiền bù nếu có.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var(o,gui,nhan)=await KhoaAsync(id,actor,ct);var gd=await db.GiaoDich.SingleAsync(x=>x.Id==o.GiaoDichId,ct);
        var receipts=await db.XacNhanGiaoNhan.Where(x=>x.GiaoDichId==o.GiaoDichId).ToListAsync(ct);
        if(receipts.Any(x=>x.NguoiXacNhanId==actor&&x.Loai==LoaiXacNhan.DaNhan)){await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);}
        KhoaTonKho.PhienBan(o.PhienBan,r.PhienBan);
        var p=await HienTaiAsync(o.GiaoDichId,ct);
        if(o.TrangThai!=TrangThaiDeNghi.DaChapNhan||gd.TrangThai!=TrangThaiGiaoDich.DangThucHien||p.TrangThai!=TrangThaiThoaThuan.DaChot||await db.DongYThoaThuan.CountAsync(x=>x.PhienBanThoaThuanId==p.Id&&x.QuyetDinh==QuyetDinhDongY.DongY,ct)!=2)
            throw new LoiTaiKhoan(409,"Chưa đủ hai bên đồng ý phiên bản hiện tại để bàn giao.");
        var partner=actor==o.NguoiGuiId?o.NguoiNhanId:o.NguoiGuiId;var own=actor==o.NguoiGuiId?o.SanPhamGuiId:o.SanPhamNhanId;var received=actor==o.NguoiGuiId?o.SanPhamNhanId:o.SanPhamGuiId;
        foreach(var kind in new[]{LoaiXacNhan.DaGiao,LoaiXacNhan.DaNhan})if(!receipts.Any(x=>x.NguoiXacNhanId==actor&&x.Loai==kind))db.Add(new XacNhanGiaoNhan{GiaoDichId=o.GiaoDichId,NguoiXacNhanId=actor,NguoiDoiTacId=partner,SanPhamId=kind==LoaiXacNhan.DaGiao?own:received,PhienBanThoaThuanId=p.Id,Loai=kind,NgayXacNhan=Now});
        var money=await db.TienBu.SingleOrDefaultAsync(x=>x.PhienBanThoaThuanId==p.Id,ct);
        if(money!=null){if(actor==money.NguoiTraId)money.NguoiTraDaXacNhan=true;else money.NguoiNhanDaXacNhan=true;}
        db.Entry(o).Property(x=>x.TrangThai).IsModified=true;
        await db.SaveChangesAsync(ct);
        var count=await db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==o.GiaoDichId&&x.PhienBanThoaThuanId==p.Id&&x.Loai==LoaiXacNhan.DaNhan,ct);
        if(count==2&&(money==null||money.NguoiTraDaXacNhan&&money.NguoiNhanDaXacNhan))
        {
            gui.TrangThai=TrangThaiTinDang.DaTraoDoi;nhan.TrangThai=TrangThaiTinDang.DaTraoDoi;gui.NgayCapNhat=Now;nhan.NgayCapNhat=Now;
            await db.GiuChoSanPham.Where(x=>x.GiaoDichId==o.GiaoDichId&&!x.DaGiaiPhong).ExecuteUpdateAsync(x=>x.SetProperty(p=>p.DaGiaiPhong,true),ct);
            await db.SaveChangesAsync(ct);gd.TrangThai=TrangThaiGiaoDich.HoanTat;gd.NgayHoanTat=Now;
        }
        Notify(o,count==2?"Trao đổi đã hoàn tất":"Đối tác đã xác nhận giao nhận","Bù tiền được hai bên xác nhận trực tiếp; không chuyển tiền qua ứng dụng.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);
    }
}
