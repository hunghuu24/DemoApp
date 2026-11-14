using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class NhatKyQuanTri : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid NguoiThucHienId { get; set; }
    public string HanhDong { get; set; } = string.Empty;
    public string LoaiDoiTuong { get; set; } = string.Empty;
    public Guid? DoiTuongId { get; set; }
    public string ChiTietJson { get; set; } = string.Empty;

    public NguoiDung NguoiThucHien { get; set; } = null!;
}
