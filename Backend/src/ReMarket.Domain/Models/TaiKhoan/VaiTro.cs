using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class VaiTro : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public string Ma { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;


}
