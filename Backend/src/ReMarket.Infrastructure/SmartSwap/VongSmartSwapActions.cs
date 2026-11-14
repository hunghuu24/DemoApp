using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed partial class VongSmartSwapService
{
    private static void MoiQuanLy(VongSmartSwap v)
    { if(v.NguoiKhoiTaoId==null||v.MaDeXuat==null)throw new LoiTaiKhoan(409,"Vòng lịch sử chưa có thông tin lời mời mới; chỉ được xem."); }
    private async Task<bool> DaBanGiaoAsync(Guid gd,CancellationToken ct)=>await db.XacNhanGiaoNhan.AnyAsync(x=>x.GiaoDichId==gd,ct)||
        await db.TienBu.AnyAsync(x=>x.GiaoDichId==gd&&(x.NguoiTraDaXacNhan||x.NguoiNhanDaXacNhan),ct);
    private static void DangMo(VongSmartSwap v,GiaoDich gd)
    { if(v.TrangThai is not(TrangThaiVong.ChoDongY or TrangThaiVong.SanSangBanGiao)||gd.TrangThai is not(TrangThaiGiaoDich.ChoDongY or TrangThaiGiaoDich.DangThucHien))throw new LoiTaiKhoan(409,"Vòng đang tranh chấp, đã bàn giao hoặc kết thúc."); }
    private async Task KetThucLockedAsync(VongSmartSwap v,Dictionary<Guid,TinDang> tins,Guid actor,string code,string reason,CancellationToken ct)
    {
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct);
        v.TrangThai=TrangThaiVong.DaHuy;v.MaKetThuc=code;v.LyDoKetThuc=reason;gd.TrangThai=TrangThaiGiaoDich.DaHuy;
        var holds=await db.GiuChoSanPham.Where(x=>x.GiaoDichId==v.GiaoDichId&&!x.DaGiaiPhong).ToListAsync(ct);
        foreach(var t in tins.Values)
        {
            if(holds.Any(h=>h.SanPhamId==t.SanPhamId)&&t.NgayXoa==null&&t.TrangThai==TrangThaiTinDang.DangGiuCho)
            {t.TrangThai=TrangThaiTinDang.DangHienThi;t.NgayCapNhat=Now;}
        }
        foreach(var hold in holds)hold.DaGiaiPhong=true;
        var chat=await KhoaChatAsync(v.GiaoDichId,ct);Event(chat,actor,"Vòng kết thúc: "+reason);
        Notify((await ThanhVienAsync(v.Id,ct)).Select(x=>x.Member.NguoiDungId),v,"Smart Swap đã kết thúc",reason);
        await db.SaveChangesAsync(ct);
    }
    private async Task<bool> HetHanLockedAsync(VongSmartSwap v,Dictionary<Guid,TinDang> tins,CancellationToken ct)
    {
        if(v.NguoiKhoiTaoId==null||v.TrangThai!=TrangThaiVong.ChoDongY||v.NgayHetHan>Now)return false;
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct);
        if(gd.TrangThai!=TrangThaiGiaoDich.ChoDongY||await DaBanGiaoAsync(v.GiaoDichId,ct))return false;
        await KetThucLockedAsync(v,tins,v.NguoiKhoiTaoId.Value,"HetHan","Hết hạn phản hồi phương án Smart Swap.",ct);return true;
    }
    public async Task<ChiTietVongSmartSwapDto> PhanHoiAsync(Guid id,Guid actor,PhanHoiSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.PhienBanThoaThuanId);
        if(r.DongY&&!r.DaDocPhuongAn)throw new LoiTaiKhoan(400,"Cần đọc và xác nhận toàn bộ phương án tiền bù.");
        if(!r.DongY&&(r.LyDo?.Trim().Length??0)<5)throw new LoiTaiKhoan(400,"Cần lý do từ chối ít nhất5 ký tự.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,tins)=await KhoaAsync(id,actor,ct);MoiQuanLy(v);
        if(await HetHanLockedAsync(v,tins,ct)){await tx.CommitAsync(ct);throw new LoiTaiKhoan(409,"Phương án đã hết hạn.");}
        var p=await HienTaiAsync(v.GiaoDichId,ct);if(p.Id!=r.PhienBanThoaThuanId)throw new LoiTaiKhoan(409,"Phiên bản đã thay đổi; hãy xem và xác nhận lại.");
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct);DangMo(v,gd);
        var votes=await db.DongYThoaThuan.Where(x=>x.PhienBanThoaThuanId==p.Id).ToListAsync(ct);
        var vote=votes.Single(x=>x.NguoiDungId==actor);
        if(r.DongY&&vote.QuyetDinh==QuyetDinhDongY.DongY){await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);}
        if(v.TrangThai!=TrangThaiVong.ChoDongY||gd.TrangThai!=TrangThaiGiaoDich.ChoDongY||p.TrangThai!=TrangThaiThoaThuan.DeXuat)
            throw new LoiTaiKhoan(409,"Vòng không còn chờ phản hồi.");
        if(!r.DongY)
        {
            vote.QuyetDinh=QuyetDinhDongY.TuChoi;vote.NgayPhanHoi=Now;
            await KetThucLockedAsync(v,tins,actor,"TuChoi",r.LyDo!.Trim(),ct);await tx.CommitAsync(ct);return await DocAsync(id,actor,ct,false);
        }
        // All stock, owners, wishes, flags and caps are rechecked under ordered product locks.
        await matching.KiemTraVongAsync(DocMa(v.MaDeXuat!),v.MaDeXuat!,v.GiaoDichId,ct);
        await KiemTraTienAsync(v,(await db.TienBu.Where(x=>x.PhienBanThoaThuanId==p.Id).Select(x=>new KhoanBuSmartSwapDto(x.NguoiTraId,x.NguoiNhanId,x.SoTien)).ToListAsync(ct)),ct);
        vote.QuyetDinh=QuyetDinhDongY.DongY;vote.NgayPhanHoi=Now;
        db.Entry(v).Property(x=>x.TrangThai).IsModified=true;
        await db.SaveChangesAsync(ct);
        var chat=await KhoaChatAsync(v.GiaoDichId,ct);
        if(votes.Count==3&&votes.All(x=>x.QuyetDinh==QuyetDinhDongY.DongY))
        {
            p.TrangThai=TrangThaiThoaThuan.DaChot;await db.SaveChangesAsync(ct);
            foreach(var tin in tins.Values)
            {
                if(!await db.GiuChoSanPham.AnyAsync(x=>x.SanPhamId==tin.SanPhamId&&x.GiaoDichId==v.GiaoDichId&&!x.DaGiaiPhong,ct))
                    db.Add(new GiuChoSanPham{GiaoDichId=v.GiaoDichId,SanPhamId=tin.SanPhamId,NgayTao=Now,NgayHetHan=Now.AddHours(48)});
                tin.TrangThai=TrangThaiTinDang.DangGiuCho;tin.NgayCapNhat=Now;
            }
            v.TrangThai=TrangThaiVong.SanSangBanGiao;v.NgayChapNhan=Now;
            await db.SaveChangesAsync(ct); // Root trigger requires ready circle + closed 3/3 version first.
            gd.TrangThai=TrangThaiGiaoDich.DangThucHien;
            Event(chat,actor,$"Đủ3/3 xác nhận phiên bản {p.SoPhienBan}. Ba món đã được giữ; giao dịch chính thức bắt đầu.");
            Notify(votes.Select(x=>x.NguoiDungId),v,"Smart Swap đã được cả ba đồng ý","Chỉ bàn giao theo phương án đã chốt hiện tại.");
        }
        else
        {
            Event(chat,actor,$"Một thành viên đã đồng ý phiên bản {p.SoPhienBan}. Chưa đủ3/3 để bắt đầu.");
            Notify(votes.Where(x=>x.NguoiDungId!=actor).Select(x=>x.NguoiDungId),v,"Có phản hồi Smart Swap mới","Một thành viên đã đồng ý; xem tiến độ trong vòng.");
        }
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);
    }
    private async Task KiemTraTienAsync(VongSmartSwap v,List<KhoanBuSmartSwapDto> money,CancellationToken ct)
    {
        var members=await ThanhVienAsync(v.Id,ct);var users=members.Select(x=>x.Member.NguoiDungId).ToHashSet();
        if(money.Count>2||money.Any(x=>x.SoTien is <=0 or >1_000_000_000_000||x.NguoiTraId==x.NguoiNhanId||!users.Contains(x.NguoiTraId)||!users.Contains(x.NguoiNhanId))||
            money.Select(x=>(x.NguoiTraId,x.NguoiNhanId)).Distinct().Count()!=money.Count)throw new LoiTaiKhoan(400,"Khoản bù phải dương, không trùng, chỉ giữa thành viên vòng.");
        var tinIds=members.Select(x=>x.TinId).ToArray();var needs=await db.NhuCauTraoDoi.Where(x=>tinIds.Contains(x.TinDangId)).ToDictionaryAsync(x=>x.TinDangId,ct);
        long paid=0,received=0;
        foreach(var m in members)
        {
            var outgoing=money.Where(x=>x.NguoiTraId==m.Member.NguoiDungId).Sum(x=>x.SoTien);
            var incoming=money.Where(x=>x.NguoiNhanId==m.Member.NguoiDungId).Sum(x=>x.SoTien);
            if(outgoing>0&&incoming>0)throw new LoiTaiKhoan(400,"Cần gộp số dư; không tạo luồng tiền vòng hoặc vừa trả vừa nhận.");
            if(!needs.TryGetValue(m.TinId,out var need)||outgoing>need.SoTienBuToiDa||((outgoing>0||incoming>0)&&!need.ChoPhepBuTien))
                throw new LoiTaiKhoan(400,"Tiền bù vượt hạn mức tổng trả hoặc có thành viên không chấp nhận bù.");
            paid+=outgoing;received+=incoming;
        }
        if(paid!=received)throw new LoiTaiKhoan(400,"Tổng tiền trả phải bằng tổng tiền nhận.");
    }
    public async Task<ChiTietVongSmartSwapDto> DeXuatAsync(Guid id,Guid actor,TaoPhuongAnSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.MaYeuCau);Key(r.PhienBanCoSoId);
        if(r.LoiNhan.Trim().Length<5)throw new LoiTaiKhoan(400,"Lời nhắn cần ít nhất5 ký tự.");
        var money=r.TienBu.Select(x=>new KhoanBuSmartSwapDto(x.NguoiTraId,x.NguoiNhanId,x.SoTien)).OrderBy(x=>x.NguoiTraId).ThenBy(x=>x.NguoiNhanId).ToList();
        var hash=Hash(new{Base=r.PhienBanCoSoId,Money=money,Text=r.LoiNhan.Trim()});
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,tins)=await KhoaAsync(id,actor,ct);MoiQuanLy(v);
        var replay=await db.PhienBanThoaThuan.AsNoTracking().SingleOrDefaultAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.NguoiTaoId==actor&&x.MaYeuCau==r.MaYeuCau,ct);
        if(replay!=null){if(replay.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã phương án đã dùng cho nội dung khác.");await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);}
        if(await HetHanLockedAsync(v,tins,ct)){await tx.CommitAsync(ct);throw new LoiTaiKhoan(409,"Phương án đã hết hạn.");}
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct);DangMo(v,gd);
        if(await DaBanGiaoAsync(v.GiaoDichId,ct))throw new LoiTaiKhoan(409,"Không thương lượng lại sau khi đã bàn giao hoặc xác nhận tiền.");
        var p=await HienTaiAsync(v.GiaoDichId,ct);if(p.Id!=r.PhienBanCoSoId)throw new LoiTaiKhoan(409,"Phương án cơ sở đã thay đổi.");
        await matching.KiemTraVongAsync(DocMa(v.MaDeXuat!),v.MaDeXuat!,v.GiaoDichId,ct);
        var old=await db.TienBu.AsNoTracking().Where(x=>x.PhienBanThoaThuanId==p.Id).ToListAsync(ct);
        var otherOld=old.Where(x=>x.NguoiTraId!=actor&&x.NguoiNhanId!=actor).Select(x=>(x.NguoiTraId,x.NguoiNhanId,x.SoTien)).ToHashSet();
        var otherNew=money.Where(x=>x.NguoiTraId!=actor&&x.NguoiNhanId!=actor).Select(x=>(x.NguoiTraId,x.NguoiNhanId,x.SoTien)).ToHashSet();
        if(!otherOld.SetEquals(otherNew))throw new LoiTaiKhoan(403,"Không được tự đề xuất sửa khoản bù giữa hai thành viên còn lại.");
        await KiemTraTienAsync(v,money,ct);
        if(old.Select(x=>(x.NguoiTraId,x.NguoiNhanId,x.SoTien)).ToHashSet().SetEquals(money.Select(x=>(x.NguoiTraId,x.NguoiNhanId,x.SoTien))))
            throw new LoiTaiKhoan(400,"Phương án tiền bù không thay đổi.");
        // Leave ready phase BEFORE superseding the closed version. Existing reservations remain in place.
        v.TrangThai=TrangThaiVong.ChoDongY;v.NgayChapNhan=null;gd.TrangThai=TrangThaiGiaoDich.ChoDongY;
        await db.SaveChangesAsync(ct);p.TrangThai=TrangThaiThoaThuan.DaThayThe;await db.SaveChangesAsync(ct);
        var chat=await KhoaChatAsync(v.GiaoDichId,ct);var users=(await ThanhVienAsync(id,ct)).Select(x=>x.Member.NguoiDungId).ToArray();
        TaoPhien(v,actor,p.SoPhienBan+1,users,money,r.LoiNhan,false,chat,r.MaYeuCau,hash);
        v.NgayHetHan=Now.AddHours(24);db.Entry(v).Property(x=>x.TrangThai).IsModified=true;
        Notify(users,v,"Phương án tiền bù Smart Swap mới","Cả ba thành viên, kể cả người đề xuất, cần xác nhận lại. Chưa được bàn giao.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await DocAsync(id,actor,ct);
    }
    public async Task<ChiTietVongSmartSwapDto> HuyAsync(Guid id,Guid actor,HuyVongSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.PhienBanThoaThuanId);if(r.LyDo.Trim().Length<5)throw new LoiTaiKhoan(400,"Lý do cần ít nhất5 ký tự.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,tins)=await KhoaAsync(id,actor,ct);MoiQuanLy(v);
        if(v.TrangThai==TrangThaiVong.DaHuy){await tx.CommitAsync(ct);return await DocAsync(id,actor,ct,false);}
        if(await HetHanLockedAsync(v,tins,ct)){await tx.CommitAsync(ct);return await DocAsync(id,actor,ct,false);}
        var gd=await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct);DangMo(v,gd);
        if((await HienTaiAsync(v.GiaoDichId,ct)).Id!=r.PhienBanThoaThuanId)throw new LoiTaiKhoan(409,"Phương án đã thay đổi; hãy xem lại trước khi hủy.");
        if(await DaBanGiaoAsync(v.GiaoDichId,ct))throw new LoiTaiKhoan(409,"Không hủy khi đã bàn giao hoặc xác nhận tiền.");
        await KetThucLockedAsync(v,tins,actor,"DaHuy",r.LyDo.Trim(),ct);await tx.CommitAsync(ct);return await DocAsync(id,actor,ct,false);
    }
    private async Task XuLyHetHanAsync(Guid id,Guid? actor,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,tins)=await KhoaAsync(id,actor,ct);
        await HetHanLockedAsync(v,tins,ct);await tx.CommitAsync(ct);
    }
    public async Task HetHanAsync(CancellationToken ct)
    {
        var ids=await db.VongSmartSwap.AsNoTracking().Where(x=>x.NguoiKhoiTaoId!=null&&x.TrangThai==TrangThaiVong.ChoDongY&&
            x.GiaoDich.TrangThai==TrangThaiGiaoDich.ChoDongY&&x.NgayHetHan<=Now).OrderBy(x=>x.NgayHetHan).Take(20).Select(x=>x.Id).ToListAsync(ct);
        foreach(var id in ids)await XuLyHetHanAsync(id,null,ct);
    }
}
