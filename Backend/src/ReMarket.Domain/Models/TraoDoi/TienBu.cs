using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class TienBu : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid PhienBanThoaThuanId { get; set; }
    public Guid GiaoDichId { get; set; }
    public Guid NguoiTraId { get; set; }
    public Guid NguoiNhanId { get; set; }
    public long SoTien { get; set; }
    public bool NguoiTraDaXacNhan { get; set; }
    public bool NguoiNhanDaXacNhan { get; set; }

    public PhienBanThoaThuan PhienBanThoaThuan { get; set; } = null!;
    public ThanhVienGiaoDich NguoiTra { get; set; } = null!;
    public ThanhVienGiaoDich NguoiNhan { get; set; } = null!;
}
