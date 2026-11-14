using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class NguoiDung : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public string Email { get; set; } = string.Empty;
    public string EmailChuanHoa { get; private set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public string MatKhauBam { get; set; } = string.Empty;
    public TrangThaiNguoiDung TrangThai { get; set; }


}
