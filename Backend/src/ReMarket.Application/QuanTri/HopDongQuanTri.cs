using System.ComponentModel.DataAnnotations;
using ReMarket.Application.SanPham;

namespace ReMarket.Application.QuanTri;

public sealed record TrangQuanTri<T>(List<T> Items,int TongSo,int Trang,int TongTrang);
public sealed record MocQuanTriDto(DateTimeOffset Ngay,string HanhDong,string NguoiThucHien,string NoiDung);
public sealed record NguoiDungQuanTriDto(Guid Id,string HoTen,string Email,string? SoDienThoai,string? TinhThanh,string TrangThai,
    string PhienBan,DateTimeOffset NgayThamGia,bool DaXacMinhEmail,bool DaXacMinhDienThoai,List<string> VaiTro,
    int SoTin,int SoGiaoDich,int SoLanKhoa,double? DiemDanhGia,int SoDanhGia);
public sealed record KhoaQuanTriDto(Guid Id,string LyDo,DateTimeOffset NgayBatDau,DateTimeOffset? NgayKetThuc,DateTimeOffset? NgayMoKhoa,string QuanTriVien);
public sealed record ChiTietNguoiDungQuanTriDto(NguoiDungQuanTriDto NguoiDung,List<KhoaQuanTriDto> Khoa,List<MocQuanTriDto> NhatKy,bool CoTheKhoa,bool CoTheMoKhoa);
public sealed record KhoaNguoiDungRequest([Required,MaxLength(32)]string PhienBan,[Required,StringLength(500,MinimumLength=5)]string LyDo,[Range(0,30)]int SoNgay);
public sealed record MoKhoaNguoiDungRequest([Required,MaxLength(32)]string PhienBan,[Required,StringLength(500,MinimumLength=5)]string LyDo,bool DaRaSoat);
public sealed record DanhMucQuanTriDto(Guid Id,string Ten,string Ma,Guid? DanhMucChaId,int ThuTu,bool DangHienThi,string BieuTuong,string PhienBan,int SoTin,int SoDanhMucCon);
public sealed record LuuDanhMucQuanTriRequest([Required,StringLength(120,MinimumLength=2)]string Ten,Guid? DanhMucChaId,[Range(0,100000)]int ThuTu,bool DangHienThi,[Required,MaxLength(30)]string BieuTuong,[MaxLength(32)]string? PhienBan=null);
public sealed record ChiTietTinQuanTriDto(TinDangDto Tin,List<MocQuanTriDto> LichSu,bool CoTheDuyet,bool CoTheAn);
public sealed record AnTinQuanTriRequest([Required,MaxLength(32)]string PhienBan,[Required,StringLength(500,MinimumLength=5)]string LyDo);
public sealed record ThanhVienQuanTriDto(Guid NguoiDungId,string HoTen,string VaiTro,bool DaGiao,bool DaNhan);
public sealed record SanPhamQuanTriDto(Guid Id,string Ten,long GiaTri,Guid ChuSoHuuId,string ChuSoHuu,string? AnhUrl);
public sealed record ChangQuanTriDto(Guid NguoiGiaoId,Guid NguoiNhanId,Guid SanPhamId);
public sealed record TienBuQuanTriDto(Guid NguoiTraId,Guid NguoiNhanId,long SoTien,bool NguoiTraDaXacNhan,bool NguoiNhanDaXacNhan);
public sealed record PhienBanQuanTriDto(Guid Id,int SoPhienBan,string NoiDung,string TrangThai,DateTimeOffset NgayTao,List<TienBuQuanTriDto> TienBu,List<DongYQuanTriDto> DongY);
public sealed record DongYQuanTriDto(Guid NguoiDungId,string QuyetDinh);
public sealed record VanChuyenQuanTriDto(Guid Id,Guid? NguoiGuiId,Guid? NguoiNhanId,string MaVanDon,string TrangThai,DateTimeOffset? NgayNhanDungHang,List<MocQuanTriDto> LichSu);
public sealed record BangChungQuanTriDto(Guid Id,string Loai,string MoTa,string LoaiMime,string Url,Guid NguoiTaiLenId,DateTimeOffset NgayTao);
public sealed record TinNhanQuanTriDto(Guid Id,long SoThuTu,Guid NguoiGuiId,string NguoiGui,string Loai,string? NoiDung,DateTimeOffset NgayTao,List<BangChungQuanTriDto> Anh);
public sealed record GiaoDichQuanTriDto(Guid Id,string Ma,string Loai,string TrangThai,DateTimeOffset NgayTao,DateTimeOffset? NgayHoanTat,
    List<ThanhVienQuanTriDto> ThanhVien,List<SanPhamQuanTriDto> SanPham,long TongTienBu,int SoNguoiNhan,int TongNguoiNhan);
public sealed record ChiTietGiaoDichQuanTriDto(GiaoDichQuanTriDto GiaoDich,Guid? VongId,string? TrangThaiVong,List<ChangQuanTriDto> Chang,
    List<PhienBanQuanTriDto> PhienBan,List<VanChuyenQuanTriDto> VanChuyen,List<BangChungQuanTriDto> BangChung,
    List<TinNhanQuanTriDto> TinNhan,int TongTinNhan,List<KhieuNaiQuanTriDto> KhieuNai,string? TrangThaiDonHang,string? TrangThaiThanhToan,long? TongTienDon,string? DiaDiemHen,DateTimeOffset? ThoiGianHen);
public sealed record KhieuNaiQuanTriDto(Guid Id,string Ma,Guid GiaoDichId,string MaGiaoDich,string LoaiGiaoDich,Guid NguoiGuiId,string NguoiGui,
    Guid NguoiBiKhieuNaiId,string NguoiBiKhieuNai,string TieuDe,string NoiDung,string TrangThai,DateTimeOffset NgayTao,DateTimeOffset? HanPhanHoi,string PhienBan);
public sealed record QuyetDinhQuanTriDto(Guid Id,string KetLuan,string BienPhap,string CanCu,string ThongBao,string HuongXuLy,bool DaXacMinhThucHien,DateTimeOffset NgayTao);
public sealed record ChiTietKhieuNaiQuanTriDto(KhieuNaiQuanTriDto KhieuNai,List<BangChungQuanTriDto> BangChung,List<MocQuanTriDto> PhanHoi,List<QuyetDinhQuanTriDto> QuyetDinh);
public sealed record TiepNhanKhieuNaiRequest([Required,MaxLength(32)]string PhienBan,[Required,StringLength(4000,MinimumLength=5)]string NoiDung);
public sealed record TaoKhieuNaiQuanTriRequest(Guid MaYeuCau,Guid NguoiGuiId,Guid NguoiBiKhieuNaiId,
    [Required,StringLength(200,MinimumLength=5)]string TieuDe,[Required,StringLength(4000,MinimumLength=10)]string NoiDung,bool DaDoiChieu);
public sealed record QuyetDinhQuanTriRequest(Guid MaYeuCau,[Required,MaxLength(32)]string PhienBan,[Required,StringLength(2000,MinimumLength=10)]string CanCu,
    [Required,StringLength(2000,MinimumLength=5)]string KetLuan,[Required,StringLength(2000,MinimumLength=5)]string BienPhap,
    [Required,StringLength(2000,MinimumLength=10)]string ThongBao,[Required,MaxLength(20)]string HuongXuLy,bool GuiThongBao);
public sealed record XacMinhQuyetDinhRequest([Required,MaxLength(32)]string PhienBan,Guid QuyetDinhId,bool DaXacMinh,
    [Required,StringLength(2000,MinimumLength=10)]string NoiDung);
public sealed record CotThongKeDto(DateOnly Ngay,int MuaBan,int TraoDoi);
public sealed record PhanBoQuanTriDto(Guid DanhMucId,string Ten,int SoTin);
public sealed record BaoCaoQuanTriDto(DateOnly TuNgay,DateOnly DenNgay,DateTimeOffset CapNhat,int TongNguoiDung,int NguoiDungMoi,int TinTiepNhan,int TinChoDuyet,
    int TinDaDuyet,int TinTuChoi,int MuaBan,int TraoDoi,int SmartSwap,int KhieuNaiMoi,int KhieuNaiDangXuLy,int KhieuNaiDaGiaiQuyet,int GiaoDichKyTruoc,
    List<CotThongKeDto> BieuDo,List<PhanBoQuanTriDto> DanhMuc);
