namespace ReMarket.Domain.Models;

public sealed class ThongTinGiaoNhanSmartSwap : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid ThanhVienVongId { get; set; }
    public Guid PhienBanThoaThuanId { get; set; }
    public Guid MaYeuCau { get; set; }
    public string BamYeuCau { get; set; } = string.Empty;
    public string TenLienHe { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string DiaChiGui { get; set; } = string.Empty;
    public string DiaChiNhan { get; set; } = string.Empty;
    public DateTimeOffset NgayXacNhan { get; set; }
    public ThanhVienVong ThanhVienVong { get; set; } = null!;
    public PhienBanThoaThuan PhienBanThoaThuan { get; set; } = null!;
}
