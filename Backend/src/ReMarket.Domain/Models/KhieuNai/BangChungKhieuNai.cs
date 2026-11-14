using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class BangChungKhieuNai : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid KhieuNaiId { get; set; }
    public Guid NguoiTaiLenId { get; set; }
    public string DuongDan { get; set; } = string.Empty;
    public string LoaiMime { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;

    public KhieuNai KhieuNai { get; set; } = null!;
    public NguoiDung NguoiTaiLen { get; set; } = null!;
}
