using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class TinNhan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid CuocTroChuyenId { get; set; }
    public Guid NguoiGuiId { get; set; }
    public Guid MaPhiaClient { get; set; }
    public LoaiTinNhan Loai { get; set; }
    public string? NoiDung { get; set; }
    public long SoThuTu { get; set; }
    public Guid? PhienBanThoaThuanId { get; set; }
    public PhienBanThoaThuan? PhienBanThoaThuan { get; set; }

    public ThanhVienTroChuyen NguoiGui { get; set; } = null!;
}
