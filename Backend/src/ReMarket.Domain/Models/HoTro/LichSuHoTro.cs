using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class LichSuHoTro : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid YeuCauHoTroId { get; set; }
    public Guid NguoiThucHienId { get; set; }
    public TrangThaiHoTro TrangThai { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public long SoThuTu { get; set; }
    public YeuCauHoTro YeuCauHoTro { get; set; } = null!;
    public NguoiDung NguoiThucHien { get; set; } = null!;
}
