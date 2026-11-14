using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class ThongBao : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid NguoiDungId { get; set; }
    public Guid? GiaoDichId { get; set; }
    public LoaiThongBao Loai { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public bool DaDoc { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
    public GiaoDich? GiaoDich { get; set; }
}
