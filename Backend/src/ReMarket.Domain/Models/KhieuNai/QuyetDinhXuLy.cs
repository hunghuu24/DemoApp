using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class QuyetDinhXuLy : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid KhieuNaiId { get; set; }
    public Guid QuanTriVienId { get; set; }
    public string KetLuan { get; set; } = string.Empty;
    public string BienPhap { get; set; } = string.Empty;
    public bool DaXacMinhThucHien { get; set; }
    public Guid? MaYeuCau { get; set; }
    public string? BamYeuCau { get; set; }
    public string CanCu { get; set; } = "";
    public string ThongBao { get; set; } = "";
    public string HuongXuLy { get; set; } = "GhiNhan";

    public KhieuNai KhieuNai { get; set; } = null!;
    public NguoiDung QuanTriVien { get; set; } = null!;
}
