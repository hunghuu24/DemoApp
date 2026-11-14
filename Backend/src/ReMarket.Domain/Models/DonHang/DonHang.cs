using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class DonHang : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid GiaoDichId { get; set; }
    public Guid NguoiMuaId { get; set; }
    public Guid NguoiBanId { get; set; }
    public string TenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string DiaChiNhan { get; set; } = string.Empty;
    public long TienHang { get; set; }
    public long PhiVanChuyen { get; set; }
    public long TongTien { get; set; }
    public TrangThaiDonHang TrangThai { get; set; }
    public Guid? PhieuThuongLuongId { get; set; }
    public Guid? MaYeuCau { get; set; }
    public string? BamYeuCau { get; set; }
    public DateTimeOffset? HanThanhToan { get; set; }
    public DateTimeOffset? NgayHuy { get; set; }
    public string? LyDoHuy { get; set; }
    public PhieuThuongLuong? PhieuThuongLuong { get; set; }

    public GiaoDich GiaoDich { get; set; } = null!;
    public ThanhVienGiaoDich NguoiMua { get; set; } = null!;
    public ThanhVienGiaoDich NguoiBan { get; set; } = null!;
}
