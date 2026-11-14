using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class ThanhVienVong : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid VongSmartSwapId { get; set; }
    public Guid GiaoDichId { get; set; }
    public Guid NguoiDungId { get; set; }
    public Guid SanPhamId { get; set; }
    public int ViTri { get; set; }
    public Guid? TinDangId { get; set; }
    public TinDang? TinDang { get; set; }

    public VongSmartSwap VongSmartSwap { get; set; } = null!;
    public ThanhVienGiaoDich ThanhVien { get; set; } = null!;
    public SanPham SanPham { get; set; } = null!;
}
