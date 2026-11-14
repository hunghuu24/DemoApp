using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class GiuChoSanPham : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid SanPhamId { get; set; }
    public Guid GiaoDichId { get; set; }
    public DateTimeOffset NgayHetHan { get; set; }
    public bool DaGiaiPhong { get; set; }

    public SanPham SanPham { get; set; } = null!;
    public GiaoDich GiaoDich { get; set; } = null!;
}
