using System.ComponentModel.DataAnnotations;

namespace ReMarket.Application.SanPham;

public sealed record LuuTinDangRequest(
    [Required, StringLength(200, MinimumLength = 3)] string Ten,
    Guid DanhMucId,
    [Range(1, 1_000_000_000_000)] long GiaBan,
    [Required, MaxLength(40)] string TinhTrang,
    [Required, StringLength(4000, MinimumLength = 10)] string MoTa,
    [Required, StringLength(100, MinimumLength = 2)] string TinhThanh,
    [Required, StringLength(500, MinimumLength = 2)] string DiaDiem,
    [MaxLength(32)] string? PhienBan,
    DieuKienTraoDoiRequest? TraoDoi = null);
public sealed record DieuKienTraoDoiRequest(
    [Required, StringLength(2000, MinimumLength = 10)] string MoTaMongMuon,
    bool ChoPhepMotMot, bool ChoPhepSmartSwap, bool ChoPhepBuTien,
    [Range(0, 1_000_000_000_000)] long SoTienBuToiDa,
    [MaxLength(1000)] string? GhiChu = null,
    TieuChiSmartSwapRequest? TieuChiSmartSwap = null);
public sealed record TieuChiSmartSwapRequest(Guid? DanhMucMongMuonId = null,
    [MaxLength(200)] string? TuKhoaMongMuon = null,
    [MaxLength(40)] string? TinhTrangToiThieu = null,
    [Range(0, 1_000_000_000_000)] long? GiaTriMongMuonTu = null,
    [Range(0, 1_000_000_000_000)] long? GiaTriMongMuonDen = null);
public sealed record TieuChiSmartSwapDto(Guid? DanhMucMongMuonId, string? TuKhoaMongMuon,
    string? TinhTrangToiThieu, long? GiaTriMongMuonTu, long? GiaTriMongMuonDen);
public sealed record NhuCauTraoDoiDto(string MoTaMongMuon, bool ChoPhepMotMot, bool ChoPhepSmartSwap,
    bool ChoPhepBuTien, long SoTienBuToiDa, string? GhiChu, TieuChiSmartSwapDto? TieuChiSmartSwap = null);
public sealed record PhienBanTinRequest([Required, MaxLength(32)] string PhienBan);
public sealed record DuyetTinRequest([Required, MaxLength(32)] string PhienBan, bool ChapNhan, [MaxLength(1000)] string? LyDo);
public sealed record DanhMucDto(Guid Id, string Ten, string Ma, Guid? DanhMucChaId, int SoTin, string BieuTuong="");
public sealed record AnhTinDto(Guid Id, string Url, int ThuTu, bool LaAnhBia);
public sealed record TinDangDto(Guid Id, Guid SanPhamId, string Ten, Guid DanhMucId, string DanhMuc,
    long? GiaBan, long GiaTriThamKhao, string TinhTrang, string TrangThai, string LoaiTin,
    string TinhThanh, string DiaDiem, string MoTa, Guid NguoiDangId, string NguoiDang,
    bool DaXacMinhDienThoai, DateTimeOffset NgayThamGia, DateTimeOffset? NgayDang,
    string PhienBan, List<AnhTinDto> Anh, int LuotXem, int SoCuocTroChuyen, string? LyDoTuChoi,
    bool CoTheChinhSua, NhuCauTraoDoiDto? TraoDoi = null);
public sealed record TrangTinDto(List<TinDangDto> Items, int TongSo, int Trang, int KichThuocTrang, int TongTrang);
public sealed record TrangChuSanPhamDto(List<DanhMucDto> DanhMuc, List<TinDangDto> NoiBat, List<TinDangDto> MoiDang);
public sealed record ThongKeTinDto(string TrangThai, int SoTin);
public sealed record TinCuaToiDto(TrangTinDto DanhSach, List<ThongKeTinDto> ThongKe);
public sealed class BoLocTin
{
    [MaxLength(100)] public string? TuKhoa { get; set; }
    public Guid? DanhMucId { get; set; }
    [Range(0, 1_000_000_000_000)] public long? GiaTu { get; set; }
    [Range(0, 1_000_000_000_000)] public long? GiaDen { get; set; }
    [MaxLength(40)] public string? TinhTrang { get; set; }
    [MaxLength(100)] public string? TinhThanh { get; set; }
    [MaxLength(40)] public string LoaiTin { get; set; } = "MuaBan";
    public bool? ChoPhepBuTien { get; set; }
    public bool? ChoPhepMotMot { get; set; }
    public bool? ChoPhepSmartSwap { get; set; }
    [MaxLength(40)] public string SapXep { get; set; } = "moinhat";
    [Range(1, 100000)] public int Trang { get; set; } = 1;
    [Range(1, 50)] public int KichThuocTrang { get; set; } = 12;
}
