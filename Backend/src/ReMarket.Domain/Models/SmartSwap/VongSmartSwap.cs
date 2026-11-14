using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class VongSmartSwap : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid GiaoDichId { get; set; }
    public TrangThaiVong TrangThai { get; set; }
    public DateTimeOffset NgayHetHan { get; set; }
    public Guid? NguoiKhoiTaoId { get; set; }
    public Guid? MaYeuCau { get; set; }
    public string? BamYeuCau { get; set; }
    public string? MaDeXuat { get; set; }
    public string? MaKetThuc { get; set; }
    public string? LyDoKetThuc { get; set; }
    public DateTimeOffset? NgayChapNhan { get; set; }

    public GiaoDich GiaoDich { get; set; } = null!;
}
