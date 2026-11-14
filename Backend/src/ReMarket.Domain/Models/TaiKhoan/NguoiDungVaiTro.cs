using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class NguoiDungVaiTro : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid NguoiDungId { get; set; }
    public Guid VaiTroId { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
    public VaiTro VaiTro { get; set; } = null!;
}
