using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class KhieuNai : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public string Ma { get; set; } = string.Empty;
    public Guid GiaoDichId { get; set; }
    public Guid NguoiGuiId { get; set; }
    public Guid NguoiBiKhieuNaiId { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string NoiDung { get; set; } = string.Empty;
    public TrangThaiKhieuNai TrangThai { get; set; }
    public DateTimeOffset? HanPhanHoi { get; set; }
    public Guid? VanChuyenMoPhongId { get; set; }
    public Guid? MaYeuCau { get; set; }
    public string? BamYeuCau { get; set; }
    public string? LoaiSuCo { get; set; }
    public VanChuyenMoPhong? VanChuyenMoPhong { get; set; }

    public GiaoDich GiaoDich { get; set; } = null!;
    public ThanhVienGiaoDich NguoiGui { get; set; } = null!;
    public ThanhVienGiaoDich NguoiBiKhieuNai { get; set; } = null!;
}
