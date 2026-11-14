using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class NhatKyKiemDuyet : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid TinDangId { get; set; }
    public Guid QuanTriVienId { get; set; }
    public TrangThaiTinDang TrangThaiTruoc { get; set; }
    public TrangThaiTinDang TrangThaiSau { get; set; }
    public string LyDo { get; set; } = string.Empty;

    public TinDang TinDang { get; set; } = null!;
    public NguoiDung QuanTriVien { get; set; } = null!;
}
