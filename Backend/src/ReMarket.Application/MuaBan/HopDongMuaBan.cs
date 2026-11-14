using System.ComponentModel.DataAnnotations;

namespace ReMarket.Application.MuaBan;

public sealed record MoChatRequest(Guid TinDangId);
public sealed record GuiTinRequest(Guid MaPhiaClient, [Required, StringLength(4000, MinimumLength = 1)] string NoiDung);
public sealed record TaoPhieuRequest(Guid MaPhiaClient, [Range(1, 1_000_000_000_000)] long SoTienGiam);
public sealed record PhanHoiPhieuRequest([Required, MaxLength(32)] string PhienBan, bool ChapNhan);
public sealed record BaoGiaRequest(Guid TinDangId, Guid? PhieuThuongLuongId);
public sealed record TaoDonRequest(Guid MaYeuCau, Guid TinDangId, [Required, MaxLength(32)] string PhienBanTin,
    Guid? PhieuThuongLuongId, [Required, StringLength(120, MinimumLength = 2)] string TenNguoiNhan,
    [Required, MaxLength(20)] string SoDienThoai, [Required, StringLength(500, MinimumLength = 10)] string DiaChi,
    [Required, MaxLength(30)] string PhuongThuc);
public sealed record ThanhToanRequest(Guid MaYeuCau, [Required, MaxLength(32)] string PhienBan, bool ThanhCong);
public sealed record ChuyenDonRequest([Required, MaxLength(32)] string PhienBan, [MaxLength(500)] string? LyDo);
public sealed record ChuyenShipRequest([Required, MaxLength(32)] string PhienBan, [Required, MaxLength(40)] string TrangThai);
public sealed record GuiDanhGiaRequest(Guid MaYeuCau, [Range(1,5)] int SoSaoNguoiBan, [Range(1,5)] int SoSaoSanPham,
    [Required, StringLength(2000, MinimumLength = 5)] string NoiDung);
public sealed record TaoHoTroRequest(Guid MaYeuCau, Guid? DonHangId, [Required, MaxLength(40)] string Loai,
    [Required, StringLength(200, MinimumLength = 5)] string TieuDe, [Required, StringLength(4000, MinimumLength = 10)] string NoiDung);
public sealed record PhanHoiHoTroRequest([Required, MaxLength(32)] string PhienBan,
    [Required, StringLength(4000, MinimumLength = 5)] string NoiDung, [MaxLength(40)] string? TrangThai);
public sealed record SanPhamMuaDto(Guid TinDangId, Guid SanPhamId, string Ten, long GiaNiemYet, string? AnhUrl,
    Guid NguoiBanId, string NguoiBan, string TinhTrang, string TrangThaiTin);
public sealed record PhieuGiamDto(Guid Id, Guid TinNhanId, Guid NguoiMuaId, Guid NguoiBanId, long GiaNiemYet,
    long SoTienGiam, long GiaSauGiam, string TrangThai, DateTimeOffset NgayHetHan, string PhienBan);
public sealed record AnhChatDto(Guid Id, string Url);
public sealed record TinNhanDto(Guid Id, long SoThuTu, Guid NguoiGuiId, string Loai, string? NoiDung,
    DateTimeOffset NgayGui, List<AnhChatDto> Anh, Guid? PhienBanThoaThuanId = null);
public sealed record HoiThoaiDto(Guid Id, string TieuDe, Guid DoiTacId, string DoiTac, Guid? NguoiMuaId,
    SanPhamMuaDto? SanPham, List<TinNhanDto> TinNhan, List<PhieuGiamDto> Phieu, bool ConTinCu, bool CoTheThuongLuong);
public sealed record HopThuMuaDto(Guid Id, string TieuDe, string? TinCuoi, DateTimeOffset NgayTao, int ChuaDoc,
    string DoiTac, string? AnhUrl);
public sealed record BaoGiaDto(SanPhamMuaDto SanPham, string PhienBanTin, long GiaNiemYet, long SoTienGiam,
    long TienHang, long PhiVanChuyen, long TongTien, Guid? PhieuThuongLuongId, List<PhieuGiamDto> PhieuKhaDung);
public sealed record MocDonDto(string TrangThai, string MoTa, DateTimeOffset? ThoiDiem);
public sealed record ThanhToanDto(string TrangThai, bool LaMoPhong, bool DangGiuTien, DateTimeOffset? NgayThanhToan,
    DateTimeOffset? NgayGiaiNgan, DateTimeOffset? NgayHoanTien);
public sealed record VanChuyenDto(string MaVanDon, string TrangThai, DateTimeOffset NgayGiaoDuKien, List<MocDonDto> LichSu);
public sealed record DanhGiaMuaDto(Guid Id, int SoSaoNguoiBan, int? SoSaoSanPham, string NoiDung, DateTimeOffset NgayTao);
public sealed record DonHangDto(Guid Id, string Ma, string PhienBan, string TrangThai, Guid NguoiMuaId, Guid NguoiBanId,
    string NguoiMua, string NguoiBan, DateTimeOffset NgayTao, DateTimeOffset? HanThanhToan, string? LyDoHuy,
    SanPhamMuaDto SanPham, long GiaNiemYet, long SoTienGiam, long TienHang, long PhiVanChuyen, long TongTien,
    string TenNguoiNhan, string SoDienThoai, string DiaChi, ThanhToanDto ThanhToan, VanChuyenDto? VanChuyen,
    List<MocDonDto> LichSu, DanhGiaMuaDto? DanhGia, bool CoTheThanhToan, bool CoTheHuy, bool CoTheNhanHang,
    bool CoTheDanhGia, List<string> BuocVanChuyen);
public sealed record TrangDonDto(List<DonHangDto> Items, int TongSo, int Trang, int TongTrang);
public sealed record MocHoTroDto(string TrangThai, string NoiDung, string NguoiGui, DateTimeOffset NgayTao);
public sealed record HoTroDto(Guid Id, string Ma, string PhienBan, Guid NguoiGuiId, Guid? DonHangId, string? MaDon,
    string Loai, string TrangThai, string TieuDe, string NoiDung, DateTimeOffset NgayTao, List<MocHoTroDto> LichSu);
