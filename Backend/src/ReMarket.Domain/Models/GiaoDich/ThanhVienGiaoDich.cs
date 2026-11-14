using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class ThanhVienGiaoDich : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid GiaoDichId { get; set; }
    public Guid NguoiDungId { get; set; }
    public string VaiTroHienThi { get; set; } = string.Empty;

    public GiaoDich GiaoDich { get; set; } = null!;
    public NguoiDung NguoiDung { get; set; } = null!;
}
