using System.ComponentModel.DataAnnotations;

namespace ReMarket.Application.TaiKhoan;

public sealed record DangKyRequest(
    [Required, StringLength(120, MinimumLength = 2)] string HoTen,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(20)] string SoDienThoai,
    [Required, StringLength(128, MinimumLength = 8)] string MatKhau,
    [Required, MaxLength(128)] string XacNhanMatKhau,
    bool DongYDieuKhoan);
public sealed record DangNhapRequest(
    [Required, MaxLength(256)] string TaiKhoan,
    [Required, MaxLength(128)] string MatKhau);
public sealed record RefreshRequest([Required, StringLength(128, MinimumLength = 32)] string RefreshToken);
public sealed record QuenMatKhauRequest([Required, EmailAddress, MaxLength(256)] string Email);
public sealed record DatLaiMatKhauRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, StringLength(128, MinimumLength = 32)] string Token,
    [Required, StringLength(128, MinimumLength = 8)] string MatKhauMoi,
    [Required, MaxLength(128)] string XacNhanMatKhau);
public sealed record DoiMatKhauRequest(
    [Required, MaxLength(128)] string MatKhauCu,
    [Required, StringLength(128, MinimumLength = 8)] string MatKhauMoi,
    [Required, MaxLength(128)] string XacNhanMatKhau);
public sealed record CapNhatHoSoRequest(
    [Required, StringLength(120, MinimumLength = 2)] string HoTen,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(20)] string SoDienThoai,
    [MaxLength(500)] string? DiaChi,
    [MaxLength(100)] string? TinhThanh,
    [Required, MaxLength(32)] string PhienBan);
public sealed record HoSoDto(Guid Id, string HoTen, string Email, string? SoDienThoai,
    string? DiaChi, string? TinhThanh, string? AnhDaiDienUrl, bool DaXacMinhEmail,
    bool DaXacMinhDienThoai, string[] VaiTro, string PhienBan, DateTimeOffset NgayThamGia);
public sealed record DangNhapDto(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenHetHan,
    DateTimeOffset RefreshTokenHetHan, HoSoDto HoSo);
public sealed record ThongBaoDto(string ThongBao);
public sealed class LoiTaiKhoan(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}
public interface ITaiKhoanService
{
    Task<HoSoDto> DangKyAsync(DangKyRequest request, CancellationToken ct);
    Task<DangNhapDto> DangNhapAsync(DangNhapRequest request, CancellationToken ct);
    Task<DangNhapDto> LamMoiAsync(string refreshToken, CancellationToken ct);
    Task DangXuatAsync(Guid nguoiDungId, Guid phienId, CancellationToken ct);
    Task QuenMatKhauAsync(string email, CancellationToken ct);
    Task DatLaiMatKhauAsync(DatLaiMatKhauRequest request, CancellationToken ct);
    Task DoiMatKhauAsync(Guid nguoiDungId, DoiMatKhauRequest request, CancellationToken ct);
    Task<HoSoDto> LayHoSoAsync(Guid nguoiDungId, CancellationToken ct);
    Task<HoSoDto> CapNhatHoSoAsync(Guid nguoiDungId, CapNhatHoSoRequest request, CancellationToken ct);
}
public interface IGuiThuKhoiPhuc
{
    Task GuiAsync(string email, string token, CancellationToken ct);
}
