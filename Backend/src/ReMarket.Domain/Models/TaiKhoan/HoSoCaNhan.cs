using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class HoSoCaNhan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid NguoiDungId { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string? AnhDaiDienUrl { get; set; }
    public string? DiaChi { get; set; }
    public string? TinhThanh { get; set; }
    public bool DaXacMinhEmail { get; set; }
    public bool DaXacMinhDienThoai { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
}
