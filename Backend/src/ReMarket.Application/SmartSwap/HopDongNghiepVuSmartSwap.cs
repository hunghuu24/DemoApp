using System.ComponentModel.DataAnnotations;
using ReMarket.Application.SanPham;

namespace ReMarket.Application.SmartSwap;

public sealed record XemVongSmartSwapRequest([Required, StringLength(98,MinimumLength=98)] string MaDeXuat);
public sealed record GuiLoiMoiSmartSwapRequest(Guid MaYeuCau,
    [Required,StringLength(98,MinimumLength=98)] string MaDeXuat,
    [Required,StringLength(64,MinimumLength=64)] string BamPhuongAn,
    [Required,StringLength(2000,MinimumLength=5)] string LoiNhan, bool DongYPhuongAn);
public sealed record PhanHoiSmartSwapRequest(Guid PhienBanThoaThuanId, bool DongY,
    bool DaDocPhuongAn, [MaxLength(500)] string? LyDo = null);
public sealed record KhoanBuSmartSwapRequest(Guid NguoiTraId, Guid NguoiNhanId,
    [Range(1,1_000_000_000_000)] long SoTien);
public sealed record TaoPhuongAnSmartSwapRequest(Guid MaYeuCau, Guid PhienBanCoSoId,
    [Required,MaxLength(2)] List<KhoanBuSmartSwapRequest> TienBu,
    [Required,StringLength(2000,MinimumLength=5)] string LoiNhan);
public sealed record HuyVongSmartSwapRequest(Guid PhienBanThoaThuanId,
    [Required,StringLength(500,MinimumLength=5)] string LyDo);
public sealed record HoSoThanhVienSmartSwapDto(Guid NguoiDungId,string HoTen);
public sealed record XemVongSmartSwapDto(VongSmartSwapDeXuatDto Vong,List<HoSoThanhVienSmartSwapDto> HoSo,
    string BamPhuongAn,bool CoTheKhoiTao);
public sealed record DongYSmartSwapDto(Guid NguoiDungId,string HoTen,string QuyetDinh,DateTimeOffset? NgayPhanHoi);
public sealed record PhuongAnSmartSwapDto(Guid Id,int SoPhienBan,Guid NguoiTaoId,string TrangThai,
    string LoiNhan,DateTimeOffset NgayTao,Guid? MaYeuCau,Guid? TinNhanId,List<KhoanBuSmartSwapDto> TienBu,
    long TongTienTra,long TongTienNhan,List<DongYSmartSwapDto> DongY);
public sealed record ThanhVienSmartSwapDto(Guid Id,int ViTri,Guid NguoiDungId,string HoTen,
    MonSmartSwapDto SanPhamCho,Guid NguoiGiaoChoToiId,Guid NguoiNhanMonCuaToiId,Guid SanPhamNhanId,
    string MoTaSanPham,NhuCauTraoDoiDto? NhuCau,string? AnhUrl);
public sealed record ChangVongSmartSwapDto(Guid NguoiGiaoId,Guid NguoiNhanId,MonSmartSwapDto SanPham);
public sealed record ChiTietVongSmartSwapDto(Guid Id,Guid GiaoDichId,string Ma,string PhienBan,
    Guid? NguoiKhoiTaoId,string TrangThai,string TrangThaiGiaoDich,DateTimeOffset NgayHetHan,
    DateTimeOffset? NgayChapNhan,string? MaKetThuc,string? LyDoKetThuc,Guid? HoiThoaiId,
    List<ThanhVienSmartSwapDto> ThanhVien,List<ChangVongSmartSwapDto> Chang,
    PhuongAnSmartSwapDto? PhuongAnHienTai,List<PhuongAnSmartSwapDto> LichSu,
    bool CoThePhanHoi,bool CoTheDeXuat,bool CoTheHuy,bool CoTheBanGiao,TienDoSmartSwapDto? TienDoGiaoNhan=null);
public sealed record TrangVongSmartSwapDto(List<ChiTietVongSmartSwapDto> Items,int TongSo,int Trang,int TongTrang);
public sealed record ThongBaoSmartSwapDto(Guid Id,Guid VongId,string TieuDe,string NoiDung,bool DaDoc,DateTimeOffset NgayTao);
