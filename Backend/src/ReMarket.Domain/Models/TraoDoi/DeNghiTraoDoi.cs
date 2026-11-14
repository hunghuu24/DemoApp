using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class DeNghiTraoDoi : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid GiaoDichId { get; set; }
    public Guid NguoiGuiId { get; set; }
    public Guid NguoiNhanId { get; set; }
    public Guid SanPhamGuiId { get; set; }
    public Guid SanPhamNhanId { get; set; }
    public TrangThaiDeNghi TrangThai { get; set; }
    public DateTimeOffset NgayHetHan { get; set; }
    public Guid? TinGuiId { get; set; }
    public Guid? TinNhanId { get; set; }
    public Guid? MaYeuCau { get; set; }
    public string? BamYeuCau { get; set; }
    public string? LyDoKetThuc { get; set; }
    public TinDang? TinGui { get; set; }
    public TinDang? TinNhan { get; set; }

    public GiaoDich GiaoDich { get; set; } = null!;
    public ThanhVienGiaoDich NguoiGui { get; set; } = null!;
    public ThanhVienGiaoDich NguoiNhan { get; set; } = null!;
    public SanPham SanPhamGui { get; set; } = null!;
    public SanPham SanPhamNhan { get; set; } = null!;
}
