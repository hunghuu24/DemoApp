using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class DongYThoaThuan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid PhienBanThoaThuanId { get; set; }
    public Guid GiaoDichId { get; set; }
    public Guid NguoiDungId { get; set; }
    public QuyetDinhDongY QuyetDinh { get; set; }
    public DateTimeOffset? NgayPhanHoi { get; set; }

    public PhienBanThoaThuan PhienBanThoaThuan { get; set; } = null!;
    public ThanhVienGiaoDich ThanhVien { get; set; } = null!;
}
