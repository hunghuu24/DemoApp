using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class DanhMuc : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public string Ten { get; set; } = string.Empty;
    public string Ma { get; set; } = string.Empty;
    public Guid? DanhMucChaId { get; set; }
    public int ThuTu { get; set; }
    public bool DangHienThi { get; set; }
    public string BieuTuong { get; set; } = "";

    public DanhMuc? DanhMucCha { get; set; }
}
