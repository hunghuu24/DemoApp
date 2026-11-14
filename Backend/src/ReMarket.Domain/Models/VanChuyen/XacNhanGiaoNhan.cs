using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class XacNhanGiaoNhan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid GiaoDichId { get; set; }
    public Guid NguoiXacNhanId { get; set; }
    public Guid NguoiDoiTacId { get; set; }
    public Guid SanPhamId { get; set; }
    public Guid? PhienBanThoaThuanId { get; set; }
    public LoaiXacNhan Loai { get; set; }
    public DateTimeOffset NgayXacNhan { get; set; }

    public GiaoDich GiaoDich { get; set; } = null!;
    public ThanhVienGiaoDich NguoiXacNhan { get; set; } = null!;
    public ThanhVienGiaoDich NguoiDoiTac { get; set; } = null!;
    public SanPham SanPham { get; set; } = null!;
    public PhienBanThoaThuan? PhienBanThoaThuan { get; set; }
}
