using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class KhoaTaiKhoan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid NguoiDungId { get; set; }
    public Guid QuanTriVienId { get; set; }
    public string LyDo { get; set; } = string.Empty;
    public DateTimeOffset NgayBatDau { get; set; }
    public DateTimeOffset? NgayKetThuc { get; set; }
    public DateTimeOffset? NgayMoKhoa { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
    public NguoiDung QuanTriVien { get; set; } = null!;
}
