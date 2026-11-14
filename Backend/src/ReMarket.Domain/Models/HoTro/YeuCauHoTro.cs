using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class YeuCauHoTro : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public string Ma { get; set; } = string.Empty;
    public Guid NguoiGuiId { get; set; }
    public Guid? DonHangId { get; set; }
    public LoaiYeuCauHoTro Loai { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public TrangThaiHoTro TrangThai { get; set; }
    public Guid? MaYeuCau { get; set; }

    public NguoiDung NguoiGui { get; set; } = null!;
    public DonHang? DonHang { get; set; }
}
