using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class DanhGia : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid GiaoDichId { get; set; }
    public Guid NguoiDanhGiaId { get; set; }
    public Guid NguoiDuocDanhGiaId { get; set; }
    public int SoSao { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public int? SoSaoSanPham { get; set; }
    public Guid? MaYeuCau { get; set; }

    public GiaoDich GiaoDich { get; set; } = null!;
    public ThanhVienGiaoDich NguoiDanhGia { get; set; } = null!;
    public ThanhVienGiaoDich NguoiDuocDanhGia { get; set; } = null!;
}
