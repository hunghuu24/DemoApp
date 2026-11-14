using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class LichSuVanChuyen : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid VanChuyenMoPhongId { get; set; }
    public TrangThaiVanChuyen TrangThai { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public DateTimeOffset ThoiDiem { get; set; }
    public long SoThuTu { get; set; }

    public VanChuyenMoPhong VanChuyenMoPhong { get; set; } = null!;
}
