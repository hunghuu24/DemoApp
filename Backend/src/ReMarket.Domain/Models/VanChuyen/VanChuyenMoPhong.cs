using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class VanChuyenMoPhong : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid? DonHangId { get; set; }
    public Guid? ChangTraoDoiId { get; set; }
    public Guid? PhienBanThoaThuanId { get; set; }
    public string? MaVanDonTuNhap { get; set; }
    public DateTimeOffset? NgayNhanDungHang { get; set; }
    public string? TenNguoiGui { get; set; }
    public string? DienThoaiGui { get; set; }
    public string? DiaChiGui { get; set; }
    public string? TenNguoiNhan { get; set; }
    public string? DienThoaiNhan { get; set; }
    public string? DiaChiNhan { get; set; }
    public string MaVanDon { get; set; } = string.Empty;
    public TrangThaiVanChuyen TrangThai { get; set; }
    public DateTimeOffset NgayGiaoDuKien { get; set; }

    public DonHang? DonHang { get; set; }
    public ChangTraoDoi? ChangTraoDoi { get; set; }
    public PhienBanThoaThuan? PhienBanThoaThuan { get; set; }
}
