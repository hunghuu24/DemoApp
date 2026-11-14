using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReMarket.Application.SmartSwap;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Commerce;

namespace ReMarket.Infrastructure.SmartSwap;

public sealed partial class VongSmartSwapService
{
    private static string DienThoai(string value)
    {
        var phone=value.Trim().Replace(" ","").Replace("-","");if(phone.StartsWith("+84",StringComparison.Ordinal))phone="0"+phone[3..];
        if(phone.Length!=10||phone[0]!='0'||!phone.All(char.IsAsciiDigit))throw new LoiTaiKhoan(400,"Điện thoại Việt Nam cần10 chữ số, bắt đầu bằng0.");return phone;
    }
    private async Task<PhienBanThoaThuan> BanGiaoHopLeAsync(VongSmartSwap v,Guid? requested,CancellationToken ct,bool paused=false)
    {
        MoiQuanLy(v);var gd=await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct);
        var active=v.TrangThai is TrangThaiVong.SanSangBanGiao or TrangThaiVong.DangBanGiao && gd.TrangThai==TrangThaiGiaoDich.DangThucHien;
        if(!active&&!(paused&&v.TrangThai==TrangThaiVong.TamDung&&gd.TrangThai==TrangThaiGiaoDich.TranhChap))
            throw new LoiTaiKhoan(409,"Vòng chưa sẵn sàng, đang tạm dừng hoặc đã kết thúc.");
        var p=await HienTaiAsync(v.GiaoDichId,ct);
        if(requested!=null&&p.Id!=requested)throw new LoiTaiKhoan(409,"Phiên bản thỏa thuận đã thay đổi.");
        if(p.TrangThai!=TrangThaiThoaThuan.DaChot||await db.DongYThoaThuan.CountAsync(x=>x.PhienBanThoaThuanId==p.Id&&x.QuyetDinh==QuyetDinhDongY.DongY,ct)!=3)
            throw new LoiTaiKhoan(409,"Chưa đủ3/3 đồng ý đúng phiên bản để giao nhận.");
        if(!paused&&await db.KhieuNai.AnyAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.VanChuyenMoPhongId!=null&&x.TrangThai!=TrangThaiKhieuNai.DaGiaiQuyet&&x.TrangThai!=TrangThaiKhieuNai.TuChoi,ct))
            throw new LoiTaiKhoan(409,"Vòng có báo cáo sự cố chưa xử lý; giao nhận đang tạm dừng.");
        return p;
    }
    private async Task<VanChuyenMoPhong> VanChuyenTrongVongAsync(Guid id,Guid shipment,CancellationToken ct)=>await db.VanChuyenMoPhong
        .Include(x=>x.ChangTraoDoi).ThenInclude(x=>x!.ThanhVienGiao).Include(x=>x.ChangTraoDoi).ThenInclude(x=>x!.ThanhVienNhan)
        .SingleOrDefaultAsync(x=>x.Id==shipment&&x.ChangTraoDoiId!=null&&x.ChangTraoDoi!.VongSmartSwapId==id,ct)
        ??throw new LoiTaiKhoan(404,"Đơn vận chuyển không thuộc vòng này.");
    private void LichSuGui(VanChuyenMoPhong ship,string text)=>db.Add(new LichSuVanChuyen{VanChuyenMoPhongId=ship.Id,TrangThai=ship.TrangThai,MoTa=text,ThoiDiem=Now,NgayTao=Now});
    public async Task<GiaoNhanSmartSwapDto> LienHeAsync(Guid id,Guid actor,XacNhanLienHeSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.MaYeuCau);Key(r.PhienBanThoaThuanId);
        if(!r.XacNhanThongTin||r.TenLienHe.Trim().Length<2||r.DiaChiGui.Trim().Length<10||r.DiaChiNhan.Trim().Length<10)
            throw new LoiTaiKhoan(400,"Cần nhập và xác nhận thông tin giao nhận đầy đủ.");
        var phone=DienThoai(r.SoDienThoai);var hash=Hash(new{r.PhienBanThoaThuanId,Ten=r.TenLienHe.Trim(),Phone=phone,Gui=r.DiaChiGui.Trim(),Nhan=r.DiaChiNhan.Trim()});
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,_)=await KhoaAsync(id,actor,ct);
        var p=await BanGiaoHopLeAsync(v,r.PhienBanThoaThuanId,ct);var member=await db.ThanhVienVong.SingleAsync(x=>x.VongSmartSwapId==id&&x.NguoiDungId==actor,ct);
        var row=await db.ThongTinGiaoNhanSmartSwap.SingleOrDefaultAsync(x=>x.ThanhVienVongId==member.Id,ct);
        if(row?.MaYeuCau==r.MaYeuCau){if(row.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã xác nhận đã dùng cho thông tin khác.");await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);}
        if(await db.VanChuyenMoPhong.AnyAsync(x=>x.ChangTraoDoi!.VongSmartSwapId==id,ct))throw new LoiTaiKhoan(409,"Thông tin đã đóng thành vận đơn; không sửa địa chỉ sau khi tạo.");
        if(row==null){row=new ThongTinGiaoNhanSmartSwap{ThanhVienVongId=member.Id,NgayTao=Now};db.Add(row);}
        else KhoaTonKho.PhienBan(row.PhienBan,r.PhienBan??"");
        row.PhienBanThoaThuanId=p.Id;row.MaYeuCau=r.MaYeuCau;row.BamYeuCau=hash;row.TenLienHe=r.TenLienHe.Trim();row.SoDienThoai=phone;
        row.DiaChiGui=r.DiaChiGui.Trim();row.DiaChiNhan=r.DiaChiNhan.Trim();row.NgayXacNhan=Now;
        Event(await KhoaChatAsync(v.GiaoDichId,ct),actor,"Một thành viên đã xác nhận thông tin giao nhận. Địa chỉ chỉ hiển thị cho người liên quan đến chặng.");
        Notify((await ThanhVienAsync(id,ct)).Select(x=>x.Member.NguoiDungId),v,"Thông tin giao nhận Smart Swap","Thành viên đã xác nhận thông tin; đủ3 người mới tạo ba vận đơn mô phỏng.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);
    }
    public async Task<GiaoNhanSmartSwapDto> TaoVanChuyenAsync(Guid id,Guid actor,TaoVanChuyenSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.PhienBanThoaThuanId);await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,tins)=await KhoaAsync(id,actor,ct);MoiQuanLy(v);
        var p=await HienTaiAsync(v.GiaoDichId,ct);if(p.Id!=r.PhienBanThoaThuanId)throw new LoiTaiKhoan(409,"Phiên bản đã thay đổi.");
        var existing=await db.VanChuyenMoPhong.CountAsync(x=>x.ChangTraoDoi!.VongSmartSwapId==id,ct);
        if(existing==3){await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);}
        await BanGiaoHopLeAsync(v,p.Id,ct);
        if(existing!=0)throw new LoiTaiKhoan(409,"Vòng chưa có bộ ba vận đơn nhất quán.");
        if(await DaBanGiaoAsync(v.GiaoDichId,ct))throw new LoiTaiKhoan(409,"Đã có ghi nhận giao nhận/tiền trước bộ vận đơn này; cần xử lý lịch sử riêng.");
        var members=await db.ThanhVienVong.Where(x=>x.VongSmartSwapId==id).ToListAsync(ct);var ids=members.Select(x=>x.Id).ToArray();
        var contacts=await db.ThongTinGiaoNhanSmartSwap.Where(x=>ids.Contains(x.ThanhVienVongId)&&x.PhienBanThoaThuanId==p.Id).ToDictionaryAsync(x=>x.ThanhVienVongId,ct);
        var legs=await db.ChangTraoDoi.Where(x=>x.VongSmartSwapId==id).ToListAsync(ct);
        if(contacts.Count!=3||members.Count!=3||legs.Count!=3)throw new LoiTaiKhoan(409,"Cần đủ ba thành viên xác nhận thông tin của phiên bản hiện tại.");
        var productIds=tins.Values.Select(x=>x.SanPhamId).ToArray();
        if(await db.GiuChoSanPham.CountAsync(x=>x.GiaoDichId==v.GiaoDichId&&!x.DaGiaiPhong&&productIds.Contains(x.SanPhamId),ct)!=3||tins.Values.Any(x=>x.TrangThai!=TrangThaiTinDang.DangGiuCho))
            throw new LoiTaiKhoan(409,"Ba sản phẩm chưa được giữ đúng cho vòng này.");
        foreach(var leg in legs)
        {
            var from=contacts[leg.ThanhVienGiaoId];var to=contacts[leg.ThanhVienNhanId];
            var ship=new VanChuyenMoPhong{ChangTraoDoiId=leg.Id,PhienBanThoaThuanId=p.Id,MaVanDon="SSSIM"+Guid.NewGuid().ToString("N")[..20].ToUpperInvariant(),
                TrangThai=TrangThaiVanChuyen.ChoGui,NgayTao=Now,NgayGiaoDuKien=Now.AddDays(3),TenNguoiGui=from.TenLienHe,DienThoaiGui=from.SoDienThoai,DiaChiGui=from.DiaChiGui,
                TenNguoiNhan=to.TenLienHe,DienThoaiNhan=to.SoDienThoai,DiaChiNhan=to.DiaChiNhan};db.Add(ship);LichSuGui(ship,"Tạo đơn mô phỏng, chờ người gửi nhập mã vận đơn và xác nhận gửi.");
        }
        v.TrangThai=TrangThaiVong.DangBanGiao;
        Event(await KhoaChatAsync(v.GiaoDichId,ct),actor,"Đã tạo đủ3 đơn vận chuyển mô phỏng. Tiến độ nhận đúng hàng0/3.");
        Notify(members.Select(x=>x.NguoiDungId),v,"Smart Swap bắt đầu giao nhận","Ba vận đơn mô phỏng đã sẵn sàng; không có đơn vị vận chuyển thật.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);
    }
    public async Task<GiaoNhanSmartSwapDto> MaVanDonAsync(Guid id,Guid shipment,Guid actor,MaVanDonSmartSwapRequest r,CancellationToken ct)
    {
        var code=r.MaVanDon.Trim().ToUpperInvariant();
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,_)=await KhoaAsync(id,actor,ct);await BanGiaoHopLeAsync(v,null,ct);
        var ship=await VanChuyenTrongVongAsync(id,shipment,ct);
        if(ship.ChangTraoDoi!.ThanhVienGiao.NguoiDungId!=actor)throw new LoiTaiKhoan(403,"Chỉ người gửi của chặng được nhập mã vận đơn.");
        if(ship.MaVanDonTuNhap==code){await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);}
        if(ship.TrangThai!=TrangThaiVanChuyen.ChoGui)throw new LoiTaiKhoan(409,"Không đổi mã sau khi đã gửi.");
        KhoaTonKho.PhienBan(ship.PhienBan,r.PhienBan);
        if(await db.VanChuyenMoPhong.AnyAsync(x=>x.Id!=ship.Id&&x.MaVanDonTuNhap==code,ct))throw new LoiTaiKhoan(409,"Mã vận đơn đã được dùng.");
        ship.MaVanDonTuNhap=code;LichSuGui(ship,"Người gửi nhập/cập nhật mã vận đơn mô phỏng.");
        try{await db.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(ex.InnerException is SqlException{Number:2601 or 2627}){throw new LoiTaiKhoan(409,"Mã vận đơn vừa được chặng khác sử dụng.");}
        await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);
    }
    public async Task<GiaoNhanSmartSwapDto> ChuyenVanChuyenAsync(Guid id,Guid shipment,Guid actor,ChuyenVanChuyenSmartSwapRequest r,CancellationToken ct)
    {
        if(r.TrangThai is not("DaGui" or "DangVanChuyen" or "DaGiao"))throw new LoiTaiKhoan(400,"Trạng thái phải là DaGui, DangVanChuyen hoặc DaGiao.");
        var next=Enum.Parse<TrangThaiVanChuyen>(r.TrangThai);
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,_)=await KhoaAsync(id,actor,ct);var p=await BanGiaoHopLeAsync(v,null,ct);
        var ship=await VanChuyenTrongVongAsync(id,shipment,ct);var leg=ship.ChangTraoDoi!;
        if(leg.ThanhVienGiao.NguoiDungId!=actor)throw new LoiTaiKhoan(403,"Chỉ người gửi được cập nhật vận chuyển mô phỏng của chặng mình.");
        if(ship.TrangThai==next){await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);}
        KhoaTonKho.PhienBan(ship.PhienBan,r.PhienBan);
        var valid=ship.TrangThai==TrangThaiVanChuyen.ChoGui&&next==TrangThaiVanChuyen.DaGui||ship.TrangThai==TrangThaiVanChuyen.DaGui&&next==TrangThaiVanChuyen.DangVanChuyen||ship.TrangThai==TrangThaiVanChuyen.DangVanChuyen&&next==TrangThaiVanChuyen.DaGiao;
        if(!valid||ship.NgayNhanDungHang!=null)throw new LoiTaiKhoan(409,"Không bỏ bước hoặc đảo trạng thái vận chuyển.");
        if(next==TrangThaiVanChuyen.DaGui&&(ship.MaVanDonTuNhap==null||!r.XacNhanDaGui))throw new LoiTaiKhoan(400,"Nhập mã và xác nhận chính bạn đã gửi món hàng.");
        ship.TrangThai=next;LichSuGui(ship,"Người gửi cập nhật mô phỏng: "+next+".");await db.SaveChangesAsync(ct);
        if(next==TrangThaiVanChuyen.DaGui)db.Add(new XacNhanGiaoNhan{GiaoDichId=v.GiaoDichId,PhienBanThoaThuanId=p.Id,NguoiXacNhanId=actor,
            NguoiDoiTacId=leg.ThanhVienNhan.NguoiDungId,SanPhamId=leg.SanPhamId,Loai=LoaiXacNhan.DaGiao,NgayXacNhan=Now,NgayTao=Now});
        Event(await KhoaChatAsync(v.GiaoDichId,ct),actor,"Cập nhật vận chuyển mô phỏng: "+next+". Người nhận cần tự xác nhận đúng hàng.");
        Notify((await ThanhVienAsync(id,ct)).Select(x=>x.Member.NguoiDungId),v,"Tiến độ vận chuyển Smart Swap",next==TrangThaiVanChuyen.DaGiao?"Vận chuyển báo đã giao; chưa tính là người nhận đã kiểm tra đúng hàng.":"Một chặng đang ở trạng thái "+next+".");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);
    }
    public async Task<GiaoNhanSmartSwapDto> NhanDungHangAsync(Guid id,Guid shipment,Guid actor,NhanHangSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.PhienBanThoaThuanId);if(!r.DaKiemTraHang||!r.NhanDungHang)throw new LoiTaiKhoan(400,"Chỉ xác nhận sau khi kiểm tra và nhận đúng hàng. Nếu có vấn đề, hãy gửi báo cáo sự cố.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,tins)=await KhoaAsync(id,actor,ct);MoiQuanLy(v);
        var p=await HienTaiAsync(v.GiaoDichId,ct);if(p.Id!=r.PhienBanThoaThuanId)throw new LoiTaiKhoan(409,"Phiên bản không còn hiện hành.");
        var ship=await VanChuyenTrongVongAsync(id,shipment,ct);var leg=ship.ChangTraoDoi!;
        if(leg.ThanhVienNhan.NguoiDungId!=actor)throw new LoiTaiKhoan(403,"Chỉ người nhận được chỉ định của chặng được xác nhận nhận đúng hàng.");
        if(ship.NgayNhanDungHang!=null){await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);}
        await BanGiaoHopLeAsync(v,p.Id,ct);
        if(ship.TrangThai!=TrangThaiVanChuyen.DaGiao)throw new LoiTaiKhoan(409,"Chặng chưa ở trạng thái đã giao.");
        var money=await db.TienBu.Where(x=>x.PhienBanThoaThuanId==p.Id&&(x.NguoiTraId==actor||x.NguoiNhanId==actor)).ToListAsync(ct);
        if(money.Count>0&&!r.DaXacNhanTienBuCuaToi)throw new LoiTaiKhoan(400,"Cần xác nhận đã trả/nhận đủ phần tiền bù của chính bạn theo phương án đã chốt.");
        db.Add(new XacNhanGiaoNhan{GiaoDichId=v.GiaoDichId,PhienBanThoaThuanId=p.Id,NguoiXacNhanId=actor,NguoiDoiTacId=leg.ThanhVienGiao.NguoiDungId,
            SanPhamId=leg.SanPhamId,Loai=LoaiXacNhan.DaNhan,NgayXacNhan=Now,NgayTao=Now});await db.SaveChangesAsync(ct);
        ship.NgayNhanDungHang=Now;foreach(var m in money){if(m.NguoiTraId==actor)m.NguoiTraDaXacNhan=true;if(m.NguoiNhanId==actor)m.NguoiNhanDaXacNhan=true;}
        await db.SaveChangesAsync(ct);
        var ships=await db.VanChuyenMoPhong.Where(x=>x.ChangTraoDoi!.VongSmartSwapId==id).ToListAsync(ct);var count=ships.Count(x=>x.NgayNhanDungHang!=null);
        if(count==3&&ships.Count==3)
        {
            if(await db.XacNhanGiaoNhan.CountAsync(x=>x.GiaoDichId==v.GiaoDichId&&x.PhienBanThoaThuanId==p.Id&&x.Loai==LoaiXacNhan.DaNhan,ct)!=3||
                await db.TienBu.AnyAsync(x=>x.PhienBanThoaThuanId==p.Id&&(!x.NguoiTraDaXacNhan||!x.NguoiNhanDaXacNhan),ct))throw new LoiTaiKhoan(409,"Chưa đủ xác nhận đúng người nhận và tiền bù.");
            foreach(var tin in tins.Values){tin.TrangThai=TrangThaiTinDang.DaTraoDoi;tin.NgayCapNhat=Now;}
            foreach(var hold in await db.GiuChoSanPham.Where(x=>x.GiaoDichId==v.GiaoDichId&&!x.DaGiaiPhong).ToListAsync(ct))hold.DaGiaiPhong=true;
            v.TrangThai=TrangThaiVong.HoanTat;await db.SaveChangesAsync(ct);
            var gd=await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct);gd.TrangThai=TrangThaiGiaoDich.HoanTat;gd.NgayHoanTat=Now;
        }
        Event(await KhoaChatAsync(v.GiaoDichId,ct),actor,$"Người nhận đã xác nhận kiểm tra đúng hàng. Tiến độ {count}/3"+(count==3?"; vòng đã hoàn tất.":"; chưa hoàn tất."));
        Notify((await ThanhVienAsync(id,ct)).Select(x=>x.Member.NguoiDungId),v,count==3?"Smart Swap đã hoàn tất":"Có xác nhận nhận đúng hàng",$"Tiến độ nhận đúng hàng {count}/3.");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);
    }
    public async Task<GiaoNhanSmartSwapDto> BaoSuCoAsync(Guid id,Guid actor,BaoSuCoSmartSwapRequest r,CancellationToken ct)
    {
        Key(r.MaYeuCau);if(r.LoaiSuCo is not("SaiHang" or "ChuaNhanHang")||r.TieuDe.Trim().Length<5||r.NoiDung.Trim().Length<10)throw new LoiTaiKhoan(400,"Chọn sự cố và mô tả đầy đủ vấn đề.");
        var hash=Hash(new{r.VanChuyenId,r.LoaiSuCo,Title=r.TieuDe.Trim(),Text=r.NoiDung.Trim()});
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(v,_)=await KhoaAsync(id,actor,ct);
        var old=await db.KhieuNai.SingleOrDefaultAsync(x=>x.NguoiGuiId==actor&&x.MaYeuCau==r.MaYeuCau,ct);
        if(old!=null){if(old.GiaoDichId!=v.GiaoDichId||old.BamYeuCau!=hash)throw new LoiTaiKhoan(409,"Mã báo cáo đã dùng cho nội dung khác.");await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);}
        await BanGiaoHopLeAsync(v,null,ct,true);var ship=await VanChuyenTrongVongAsync(id,r.VanChuyenId,ct);var leg=ship.ChangTraoDoi!;
        if(leg.ThanhVienNhan.NguoiDungId!=actor)throw new LoiTaiKhoan(403,"Chỉ người nhận của đơn được báo sự cố nhận hàng tương ứng.");
        if(ship.NgayNhanDungHang!=null)throw new LoiTaiKhoan(409,"Chặng đã được bạn xác nhận đúng hàng; cần quy trình hỗ trợ sau xác nhận riêng.");
        if(r.LoaiSuCo=="SaiHang"&&ship.TrangThai==TrangThaiVanChuyen.ChoGui)throw new LoiTaiKhoan(409,"Chưa có xác nhận gửi để báo nhận sai hàng.");
        db.Add(new KhieuNai{Ma="SSKN"+Guid.NewGuid().ToString("N")[..18].ToUpperInvariant(),GiaoDichId=v.GiaoDichId,VanChuyenMoPhongId=ship.Id,
            NguoiGuiId=actor,NguoiBiKhieuNaiId=leg.ThanhVienGiao.NguoiDungId,LoaiSuCo=r.LoaiSuCo,TieuDe=r.TieuDe.Trim(),NoiDung=r.NoiDung.Trim(),
            TrangThai=TrangThaiKhieuNai.Moi,MaYeuCau=r.MaYeuCau,BamYeuCau=hash,NgayTao=Now,HanPhanHoi=Now.AddHours(48)});
        v.TrangThai=TrangThaiVong.TamDung;(await db.GiaoDich.SingleAsync(x=>x.Id==v.GiaoDichId,ct)).TrangThai=TrangThaiGiaoDich.TranhChap;
        Event(await KhoaChatAsync(v.GiaoDichId,ct),actor,"Có báo cáo sự cố giao nhận. Toàn vòng tạm dừng, không cập nhật vận chuyển hoặc xác nhận nhận hàng tiếp.");
        Notify((await ThanhVienAsync(id,ct)).Select(x=>x.Member.NguoiDungId),v,"Smart Swap tạm dừng vì sự cố",r.TieuDe.Trim());
        try{await db.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(ex.InnerException is SqlException{Number:2601 or 2627})
        {throw new LoiTaiKhoan(409,"Mã báo cáo vừa được sử dụng trong yêu cầu khác; hãy tải lại và giữ đúng mã/nội dung.");}
        await tx.CommitAsync(ct);return await GiaoNhanAsync(id,actor,ct);
    }
}
