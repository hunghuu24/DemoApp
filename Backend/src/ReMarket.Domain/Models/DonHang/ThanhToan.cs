using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class ThanhToan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid DonHangId { get; set; }
    public long SoTien { get; set; }
    public TrangThaiThanhToan TrangThai { get; set; }
    public bool LaMoPhong { get; set; }
    public string? MaThamChieu { get; set; }
    public Guid? MaYeuCauCuoi { get; set; }
    public bool? KetQuaCuoi { get; set; }
    public DateTimeOffset? NgayThanhToan { get; set; }
    public DateTimeOffset? NgayGiaiNgan { get; set; }
    public DateTimeOffset? NgayHoanTien { get; set; }

    public DonHang DonHang { get; set; } = null!;
}
