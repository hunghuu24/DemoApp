namespace ReMarket.Domain.Models;

public sealed class PhienDangNhap : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid NguoiDungId { get; set; }
    public string MaTruyCap { get; set; } = string.Empty;
    public string BamRefreshToken { get; set; } = string.Empty;
    public DateTimeOffset NgayHetHan { get; set; }
    public DateTimeOffset? NgayThuHoi { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
}
