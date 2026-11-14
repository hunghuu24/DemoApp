using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class PhienBanThoaThuan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid GiaoDichId { get; set; }
    public int SoPhienBan { get; set; }
    public Guid NguoiTaoId { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? DiaDiemHen { get; set; }
    public DateTimeOffset? ThoiGianHen { get; set; }
    public TrangThaiThoaThuan TrangThai { get; set; }
    public Guid? MaYeuCau { get; set; }
    public string? BamYeuCau { get; set; }

    public GiaoDich GiaoDich { get; set; } = null!;
    public ThanhVienGiaoDich NguoiTao { get; set; } = null!;
}
