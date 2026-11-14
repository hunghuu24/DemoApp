using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class BangChungGiaoNhan : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid XacNhanGiaoNhanId { get; set; }
    public Guid NguoiTaiLenId { get; set; }
    public string DuongDan { get; set; } = string.Empty;
    public string LoaiMime { get; set; } = string.Empty;
    public Guid? MaYeuCau { get; set; }
    public string? BamNoiDung { get; set; }

    public XacNhanGiaoNhan XacNhanGiaoNhan { get; set; } = null!;
    public NguoiDung NguoiTaiLen { get; set; } = null!;
}
