using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class TepTinNhan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid TinNhanId { get; set; }
    public string DuongDan { get; set; } = string.Empty;
    public string TenTep { get; set; } = string.Empty;
    public string LoaiMime { get; set; } = string.Empty;
    public long DungLuong { get; set; }

    public TinNhan TinNhan { get; set; } = null!;
}
