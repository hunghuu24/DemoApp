using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class GiaoDich : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public string Ma { get; set; } = string.Empty;
    public LoaiGiaoDich Loai { get; set; }
    public TrangThaiGiaoDich TrangThai { get; set; }
    public DateTimeOffset? NgayHoanTat { get; set; }


}
