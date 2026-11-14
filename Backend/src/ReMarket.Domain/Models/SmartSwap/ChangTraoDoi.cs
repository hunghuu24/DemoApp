using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class ChangTraoDoi : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid VongSmartSwapId { get; set; }
    public Guid ThanhVienGiaoId { get; set; }
    public Guid ThanhVienNhanId { get; set; }
    public Guid SanPhamId { get; set; }

    public VongSmartSwap VongSmartSwap { get; set; } = null!;
    public ThanhVienVong ThanhVienGiao { get; set; } = null!;
    public ThanhVienVong ThanhVienNhan { get; set; } = null!;
}
