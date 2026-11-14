using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class TinDang : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid SanPhamId { get; set; }
    public Guid NguoiDangId { get; set; }
    public LoaiTinDang LoaiTin { get; set; }
    public long? GiaBan { get; set; }
    public TrangThaiTinDang TrangThai { get; set; }
    public string TinhThanh { get; set; } = string.Empty;
    public string DiaDiem { get; set; } = string.Empty;
    public DateTimeOffset? NgayDang { get; set; }
    public Guid? NguoiDuyetId { get; set; }
    public string? LyDoTuChoi { get; set; }
    public DateTimeOffset? NgayCapNhat { get; set; }
    public DateTimeOffset? NgayXoa { get; set; }
    public int LuotXem { get; set; }

    public SanPham SanPham { get; set; } = null!;
    public NguoiDung? NguoiDuyet { get; set; }
}
