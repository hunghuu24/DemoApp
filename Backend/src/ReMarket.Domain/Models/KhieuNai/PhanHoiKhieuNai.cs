using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class PhanHoiKhieuNai : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid KhieuNaiId { get; set; }
    public Guid NguoiGuiId { get; set; }
    public string NoiDung { get; set; } = string.Empty;

    public KhieuNai KhieuNai { get; set; } = null!;
    public NguoiDung NguoiGui { get; set; } = null!;
}
