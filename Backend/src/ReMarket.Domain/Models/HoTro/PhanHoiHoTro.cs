using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class PhanHoiHoTro : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid YeuCauHoTroId { get; set; }
    public Guid NguoiGuiId { get; set; }
    public string NoiDung { get; set; } = string.Empty;

    public YeuCauHoTro YeuCauHoTro { get; set; } = null!;
    public NguoiDung NguoiGui { get; set; } = null!;
}
