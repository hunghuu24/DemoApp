using System.ComponentModel.DataAnnotations;

namespace ReMarket.Application.SmartSwap;

public sealed record XacNhanLienHeSmartSwapRequest(Guid MaYeuCau,Guid PhienBanThoaThuanId,
    [Required,StringLength(120,MinimumLength=2)]string TenLienHe,[Required,MaxLength(20)]string SoDienThoai,
    [Required,StringLength(500,MinimumLength=10)]string DiaChiGui,[Required,StringLength(500,MinimumLength=10)]string DiaChiNhan,
    bool XacNhanThongTin,[MaxLength(32)]string? PhienBan=null);
public sealed record TaoVanChuyenSmartSwapRequest(Guid PhienBanThoaThuanId);
public sealed record MaVanDonSmartSwapRequest([Required,MaxLength(32)]string PhienBan,
    [Required,RegularExpression("^[A-Za-z0-9][A-Za-z0-9_-]{3,39}$")]string MaVanDon);
public sealed record ChuyenVanChuyenSmartSwapRequest([Required,MaxLength(32)]string PhienBan,
    [Required,MaxLength(40)]string TrangThai,bool XacNhanDaGui=false);
public sealed record NhanHangSmartSwapRequest(Guid PhienBanThoaThuanId,bool DaKiemTraHang,bool NhanDungHang,bool DaXacNhanTienBuCuaToi=false);
public sealed record BaoSuCoSmartSwapRequest(Guid MaYeuCau,Guid VanChuyenId,
    [Required,MaxLength(40)]string LoaiSuCo,[Required,StringLength(200,MinimumLength=5)]string TieuDe,
    [Required,StringLength(4000,MinimumLength=10)]string NoiDung);
public sealed record LienHeSmartSwapDto(Guid NguoiDungId,string HoTen,bool DaXacNhan,string? PhienBan,
    string? TenLienHe,string? SoDienThoai,string? DiaChiGui,string? DiaChiNhan);
public sealed record LichSuGuiHangSmartSwapDto(long SoThuTu,string TrangThai,string MoTa,DateTimeOffset ThoiDiem);
public sealed record VanChuyenSmartSwapDto(Guid Id,Guid ChangTraoDoiId,Guid NguoiGuiId,Guid NguoiNhanId,Guid SanPhamId,
    string TenSanPham,string MaMoPhong,string? MaVanDon,string TrangThai,string PhienBan,DateTimeOffset? NgayNhanDungHang,
    string? TenNguoiGui,string? DienThoaiGui,string? DiaChiGui,string? TenNguoiNhan,string? DienThoaiNhan,string? DiaChiNhan,
    List<LichSuGuiHangSmartSwapDto> LichSu,bool CoTheNhapMa,bool CoTheCapNhat,bool CoTheNhanDungHang,bool CoTheBaoSuCo);
public sealed record SuCoSmartSwapDto(Guid Id,Guid VanChuyenId,Guid NguoiBaoId,string Ma,string LoaiSuCo,
    string TieuDe,string NoiDung,string TrangThai,DateTimeOffset NgayTao);
public sealed record TienDoSmartSwapDto(int SoNguoiNhanDung,int TongSoNguoi,string TienDo,bool TamDung,bool HoanTat);
public sealed record GiaoNhanSmartSwapDto(Guid VongId,Guid? PhienBanThoaThuanId,string TrangThaiVong,string TrangThaiGiaoDich,
    int SoNguoiXacNhanThongTin,TienDoSmartSwapDto TienDo,List<LienHeSmartSwapDto> LienHe,
    List<VanChuyenSmartSwapDto> VanChuyen,List<SuCoSmartSwapDto> SuCo,bool CoTheTaoVanChuyen,List<BangChungSmartSwapDto>? BangChung=null);
public sealed record BangChungSmartSwapDto(Guid Id,Guid NguoiTaiLenId,string Url);
