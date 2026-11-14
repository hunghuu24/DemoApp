using System.ComponentModel.DataAnnotations;
using ReMarket.Application.SanPham;

namespace ReMarket.Application.TraoDoi;

public sealed record TaoDeNghiRequest(Guid MaYeuCau, Guid TinGuiId, Guid TinNhanId,
    [Required,MaxLength(32)] string PhienBanTinGui,[Required,MaxLength(32)] string PhienBanTinNhan,
    [Range(0,1_000_000_000_000)] long SoTienBu, Guid? NguoiTraId,
    [Required,StringLength(2000,MinimumLength=5)] string LoiNhan,
    [MaxLength(500)] string? DiaDiemHen, DateTimeOffset? ThoiGianHen, bool DongYQuyDinh);
public sealed record DoiDeNghiRequest([Required,MaxLength(32)] string PhienBan,
    [Range(0,1_000_000_000_000)] long SoTienBu,Guid? NguoiTraId,
    [Required,StringLength(2000,MinimumLength=5)] string LoiNhan,
    [MaxLength(500)] string? DiaDiemHen,DateTimeOffset? ThoiGianHen);
public sealed record PhanHoiDeNghiRequest([Required,MaxLength(32)] string PhienBan,bool DongY,[MaxLength(500)] string? LyDo=null);
public sealed record HuyDeNghiRequest([Required,MaxLength(32)] string PhienBan,[Required,StringLength(500,MinimumLength=5)] string LyDo);
public sealed record GiaoNhanTraoDoiRequest([Required,MaxLength(32)] string PhienBan,bool DaKiemTra,bool DaGiaoMonCuaToi,bool DaNhanDu,bool DaXacNhanTienBu);
public sealed record DanhGiaTraoDoiRequest(Guid MaYeuCau,[Range(1,5)] int SoSao,
    [Required,StringLength(2000,MinimumLength=5)] string NoiDung,[MaxLength(10)] List<Guid> TieuChiIds);
public sealed record TieuChiTraoDoiDto(Guid Id,string Ma,string Ten);
public sealed record MonTraoDoiDto(Guid SanPhamId,Guid? TinDangId,string Ten,long GiaTriThamKhao,string TinhTrang,
    Guid NguoiDungId,string ChuSoHuu,string? AnhUrl);
public sealed record DongYTraoDoiDto(Guid NguoiDungId,string HoTen,string QuyetDinh,DateTimeOffset? NgayPhanHoi);
public sealed record PhienTraoDoiDto(Guid Id,int SoPhienBan,Guid NguoiTaoId,string NguoiTao,string TrangThai,string LoiNhan,
    string? DiaDiemHen,DateTimeOffset? ThoiGianHen,long SoTienBu,Guid? NguoiTraId,Guid? NguoiNhanTienId,
    bool NguoiTraDaXacNhan,bool NguoiNhanDaXacNhan,DateTimeOffset NgayTao,List<DongYTraoDoiDto> DongY);
public sealed record XacNhanTraoDoiDto(Guid NguoiDungId,string HoTen,bool DaGiao,bool DaNhan,DateTimeOffset? NgayNhan);
public sealed record BangChungTraoDoiDto(Guid Id,Guid NguoiTaiLenId,string Url);
public sealed record DanhGiaTraoDoiDto(Guid Id,Guid NguoiDanhGiaId,int SoSao,string NoiDung,DateTimeOffset NgayTao,List<Guid> TieuChiIds);
public sealed record DeNghiTraoDoiDto(Guid Id,Guid GiaoDichId,string Ma,string PhienBan,string TrangThai,string TrangThaiGiaoDich,
    Guid NguoiGuiId,Guid NguoiNhanId,DateTimeOffset NgayTao,DateTimeOffset NgayHetHan,DateTimeOffset? NgayHoanTat,string? LyDoKetThuc,
    MonTraoDoiDto MonGui,MonTraoDoiDto MonNhan,PhienTraoDoiDto? ThoaThuan,List<PhienTraoDoiDto> LichSu,
    List<XacNhanTraoDoiDto> XacNhan,List<BangChungTraoDoiDto> BangChung,List<DanhGiaTraoDoiDto> DanhGia,
    bool CoTheDongY,bool CoTheTuChoi,bool CoTheDeXuatLai,bool CoTheHuy,bool CoTheGiaoNhan,bool CoTheDanhGia,
    bool SanPhamConKhaDung,bool DaXacNhanCuaToi);
public sealed record TrangDeNghiDto(List<DeNghiTraoDoiDto> Items,int TongSo,int Trang,int TongTrang);
