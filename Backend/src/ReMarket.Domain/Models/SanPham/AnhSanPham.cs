using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class AnhSanPham : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid SanPhamId { get; set; }
    public string DuongDan { get; set; } = string.Empty;
    public int ThuTu { get; set; }
    public bool LaAnhBia { get; set; }

    public SanPham SanPham { get; set; } = null!;
}
