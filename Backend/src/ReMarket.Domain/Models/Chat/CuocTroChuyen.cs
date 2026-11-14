using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class CuocTroChuyen : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid? GiaoDichId { get; set; }
    public Guid? TinDangId { get; set; }
    public LoaiTroChuyen Loai { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public Guid? NguoiMuaId { get; set; }

    public GiaoDich? GiaoDich { get; set; }
    public TinDang? TinDang { get; set; }
}
