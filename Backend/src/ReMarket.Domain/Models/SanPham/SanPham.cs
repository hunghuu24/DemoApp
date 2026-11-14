using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class SanPham : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid ChuSoHuuId { get; set; }
    public Guid DanhMucId { get; set; }
    public string Ten { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public TinhTrangSanPham TinhTrang { get; set; }
    public long GiaTriThamKhao { get; set; }

    public NguoiDung ChuSoHuu { get; set; } = null!;
    public DanhMuc DanhMuc { get; set; } = null!;
}
