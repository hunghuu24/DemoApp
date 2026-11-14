using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class DanhGiaTieuChi : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid DanhGiaId { get; set; }
    public Guid TieuChiDanhGiaId { get; set; }

    public DanhGia DanhGia { get; set; } = null!;
    public TieuChiDanhGia TieuChiDanhGia { get; set; } = null!;
}
