using Microsoft.EntityFrameworkCore;
using ReMarket.Application.TaiKhoan;
using ReMarket.Application.TraoDoi;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Exchange;

public sealed partial class TraoDoiService
{
    public async Task<DeNghiTraoDoiDto> DocAsync(Guid id,Guid actor,CancellationToken ct,bool expire=true)
    {
        var o=await CuaToiAsync(id,actor,ct);
        var gd=await db.GiaoDich.AsNoTracking().SingleAsync(x=>x.Id==o.GiaoDichId,ct);
        if(expire&&gd.TrangThai==TrangThaiGiaoDich.ChoDongY&&Pending(o)&&o.NgayHetHan<=Now)return await KetThucAsync(id,actor,null,"Hết hạn phản hồi đề nghị",true,false,ct);
        var ids=await TinIdsAsync(o,ct);
        var people=await db.HoSoCaNhan.AsNoTracking().Where(x=>x.NguoiDungId==o.NguoiGuiId||x.NguoiDungId==o.NguoiNhanId).ToDictionaryAsync(x=>x.NguoiDungId,x=>x.HoTen,ct);
        string Name(Guid user)=>people.GetValueOrDefault(user,"Thành viên ReMarket");
        async Task<MonTraoDoiDto> Mon(Guid productId,Guid tinId,Guid owner)
        {
            var p=await db.SanPham.AsNoTracking().SingleAsync(x=>x.Id==productId,ct);
            var picture=await db.AnhSanPham.AnyAsync(x=>x.SanPhamId==productId,ct);
            return new(productId,tinId,p.Ten,p.GiaTriThamKhao,p.TinhTrang.ToString(),owner,Name(owner),picture?$"api/traodoi/{id}/anh/{productId}":null);
        }
        var versions=await db.PhienBanThoaThuan.AsNoTracking().Where(x=>x.GiaoDichId==o.GiaoDichId).OrderBy(x=>x.SoPhienBan).ToListAsync(ct);
        var consents=await db.DongYThoaThuan.AsNoTracking().Where(x=>x.GiaoDichId==o.GiaoDichId).ToListAsync(ct);
        var money=await db.TienBu.AsNoTracking().Where(x=>x.GiaoDichId==o.GiaoDichId).ToListAsync(ct);
        var history=versions.Select(p=>{
            var m=money.SingleOrDefault(x=>x.PhienBanThoaThuanId==p.Id);
            return new PhienTraoDoiDto(p.Id,p.SoPhienBan,p.NguoiTaoId,Name(p.NguoiTaoId),p.TrangThai.ToString(),p.NoiDung,p.DiaDiemHen,p.ThoiGianHen,
                m?.SoTien??0,m?.NguoiTraId,m?.NguoiNhanId,m?.NguoiTraDaXacNhan??false,m?.NguoiNhanDaXacNhan??false,p.NgayTao,
                consents.Where(x=>x.PhienBanThoaThuanId==p.Id).OrderBy(x=>x.NguoiDungId).Select(x=>new DongYTraoDoiDto(x.NguoiDungId,Name(x.NguoiDungId),x.QuyetDinh.ToString(),x.NgayPhanHoi)).ToList());
        }).ToList();
        var current=history.LastOrDefault(x=>x.TrangThai!="DaThayThe");
        var receipts=await db.XacNhanGiaoNhan.AsNoTracking().Where(x=>x.GiaoDichId==o.GiaoDichId).ToListAsync(ct);
        var ownReceived=receipts.Any(x=>x.NguoiXacNhanId==actor&&x.Loai==LoaiXacNhan.DaNhan);
        var receiptIds=receipts.Select(x=>x.Id).ToArray();
        var evidence=await db.BangChungGiaoNhan.AsNoTracking().Where(x=>receiptIds.Contains(x.XacNhanGiaoNhanId)).OrderBy(x=>x.NgayTao).ThenBy(x=>x.Id)
            .Select(x=>new BangChungTraoDoiDto(x.Id,x.NguoiTaiLenId,$"api/traodoi/{id}/bangchung/{x.Id}")).ToListAsync(ct);
        var reviews=await db.DanhGia.AsNoTracking().Where(x=>x.GiaoDichId==o.GiaoDichId).OrderBy(x=>x.NgayTao).ToListAsync(ct);
        var reviewIds=reviews.Select(x=>x.Id).ToArray();var tags=await db.DanhGiaTieuChi.Where(x=>reviewIds.Contains(x.DanhGiaId)).ToListAsync(ct);
        var ratings=reviews.Select(x=>new DanhGiaTraoDoiDto(x.Id,x.NguoiDanhGiaId,x.SoSao,x.NoiDung,x.NgayTao,tags.Where(t=>t.DanhGiaId==x.Id).Select(t=>t.TieuChiDanhGiaId).ToList())).ToList();
        var listings=await db.TinDang.AsNoTracking().Where(x=>x.Id==ids.Gui||x.Id==ids.Nhan).ToListAsync(ct);
        var holds=await db.GiuChoSanPham.AsNoTracking().Where(x=>(x.SanPhamId==o.SanPhamGuiId||x.SanPhamId==o.SanPhamNhanId)&&!x.DaGiaiPhong).ToListAsync(ct);
        var available=listings.All(t=>t.NgayXoa==null&&(t.TrangThai==TrangThaiTinDang.DangHienThi||t.TrangThai==TrangThaiTinDang.DangGiuCho&&holds.Any(h=>h.SanPhamId==t.SanPhamId&&h.GiaoDichId==o.GiaoDichId)))&&holds.All(x=>x.GiaoDichId==o.GiaoDichId);
        var open=Pending(o)&&o.NgayHetHan>Now&&gd.TrangThai==TrangThaiGiaoDich.ChoDongY;var noHandover=receipts.Count==0;var finished=gd.TrangThai==TrangThaiGiaoDich.HoanTat;
        var otherProposal=current?.NguoiTaoId!=actor;var active=gd.TrangThai is TrangThaiGiaoDich.ChoDongY or TrangThaiGiaoDich.DangThucHien;
        return new(o.Id,o.GiaoDichId,gd.Ma,Convert.ToBase64String(o.PhienBan),o.TrangThai.ToString(),gd.TrangThai.ToString(),o.NguoiGuiId,o.NguoiNhanId,o.NgayTao,o.NgayHetHan,gd.NgayHoanTat,o.LyDoKetThuc,
            await Mon(o.SanPhamGuiId,ids.Gui,o.NguoiGuiId),await Mon(o.SanPhamNhanId,ids.Nhan,o.NguoiNhanId),current,history,
            new[]{o.NguoiGuiId,o.NguoiNhanId}.Select(user=>new XacNhanTraoDoiDto(user,Name(user),receipts.Any(x=>x.NguoiXacNhanId==user&&x.Loai==LoaiXacNhan.DaGiao),receipts.Any(x=>x.NguoiXacNhanId==user&&x.Loai==LoaiXacNhan.DaNhan),receipts.FirstOrDefault(x=>x.NguoiXacNhanId==user&&x.Loai==LoaiXacNhan.DaNhan)?.NgayXacNhan)).ToList(),evidence,ratings,
            open&&otherProposal&&available,open&&otherProposal,active&&available&&noHandover&&(open||o.TrangThai==TrangThaiDeNghi.DaChapNhan&&!finished),
            active&&noHandover&&!finished&&(open||o.TrangThai==TrangThaiDeNghi.DaChapNhan),!ownReceived&&o.TrangThai==TrangThaiDeNghi.DaChapNhan&&gd.TrangThai==TrangThaiGiaoDich.DangThucHien&&current?.TrangThai=="DaChot",
            finished&&reviews.All(x=>x.NguoiDanhGiaId!=actor),available,ownReceived);
    }
    public async Task<TrangDeNghiDto> DanhSachAsync(Guid actor,string tab,int page,CancellationToken ct)
    {
        if(tab is not("gui" or "nhan" or "thuongluong" or "hoantat" or "tatca"))throw new LoiTaiKhoan(400,"Tab trao đổi không hợp lệ.");
        var expired=await db.DeNghiTraoDoi.AsNoTracking().Where(x=>x.GiaoDich.TrangThai==TrangThaiGiaoDich.ChoDongY&&(x.NguoiGuiId==actor||x.NguoiNhanId==actor)&&(x.TrangThai==TrangThaiDeNghi.ChoPhanHoi||x.TrangThai==TrangThaiDeNghi.DeXuatLai)&&x.NgayHetHan<=Now).Select(x=>x.Id).ToListAsync(ct);
        foreach(var id in expired)await KetThucAsync(id,actor,null,"Hết hạn phản hồi đề nghị",true,false,ct);
        var query=db.DeNghiTraoDoi.AsNoTracking().Where(x=>x.NguoiGuiId==actor||x.NguoiNhanId==actor);
        query=tab switch{"gui"=>query.Where(x=>x.NguoiGuiId==actor),"nhan"=>query.Where(x=>x.NguoiNhanId==actor),"thuongluong"=>query.Where(x=>x.TrangThai==TrangThaiDeNghi.DeXuatLai&&x.GiaoDich.TrangThai!=TrangThaiGiaoDich.HoanTat),"hoantat"=>query.Where(x=>x.GiaoDich.TrangThai==TrangThaiGiaoDich.HoanTat),_=>query};
        var count=await query.CountAsync(ct);var ids=await query.OrderByDescending(x=>x.NgayTao).ThenByDescending(x=>x.Id).Skip((page-1)*10).Take(10).Select(x=>x.Id).ToListAsync(ct);
        var rows=new List<DeNghiTraoDoiDto>();foreach(var id in ids)rows.Add(await DocAsync(id,actor,ct,false));return new(rows,count,page,(int)Math.Ceiling(count/10d));
    }
    public async Task<Guid> ChatAsync(Guid id,Guid actor,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);var(o,_,_)=await KhoaAsync(id,actor,ct);
        var chat=await db.CuocTroChuyen.SingleOrDefaultAsync(x=>x.GiaoDichId==o.GiaoDichId,ct);
        if(chat==null)
        {
            var code=await db.GiaoDich.Where(x=>x.Id==o.GiaoDichId).Select(x=>x.Ma).SingleAsync(ct);
            chat=new CuocTroChuyen{GiaoDichId=o.GiaoDichId,Loai=LoaiTroChuyen.TrucTiep,TieuDe="Trao đổi 1–1 · "+code};db.Add(chat);
            db.AddRange(new ThanhVienTroChuyen{CuocTroChuyenId=chat.Id,NguoiDungId=o.NguoiGuiId},new ThanhVienTroChuyen{CuocTroChuyenId=chat.Id,NguoiDungId=o.NguoiNhanId});await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);return chat.Id;
    }
    public async Task<string> AnhAsync(Guid id,Guid product,Guid actor,CancellationToken ct)
    {
        var o=await CuaToiAsync(id,actor,ct);if(product!=o.SanPhamGuiId&&product!=o.SanPhamNhanId)throw new LoiTaiKhoan(404,"Ảnh không thuộc đề nghị.");
        var path=await db.AnhSanPham.Where(x=>x.SanPhamId==product).OrderByDescending(x=>x.LaAnhBia).ThenBy(x=>x.ThuTu).Select(x=>x.DuongDan).FirstOrDefaultAsync(ct)??throw new LoiTaiKhoan(404,"Không tìm thấy ảnh.");
        var full=images.DuongDan(path);if(!File.Exists(full))throw new LoiTaiKhoan(404,"Không tìm thấy ảnh.");return full;
    }
    public Task<List<TieuChiTraoDoiDto>> TieuChiAsync(CancellationToken ct)=>db.TieuChiDanhGia.AsNoTracking().OrderBy(x=>x.Ma).Select(x=>new TieuChiTraoDoiDto(x.Id,x.Ma,x.Ten)).ToListAsync(ct);
}
