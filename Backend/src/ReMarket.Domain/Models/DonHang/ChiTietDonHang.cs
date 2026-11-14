using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class ChiTietDonHang : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid DonHangId { get; set; }
    public Guid TinDangId { get; set; }
    public Guid SanPhamId { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public long DonGia { get; set; }
    public int SoLuong { get; set; }
    public long? GiaNiemYet { get; set; }
    public long SoTienGiam { get; set; }

    public DonHang DonHang { get; set; } = null!;
    public TinDang TinDang { get; set; } = null!;
}
