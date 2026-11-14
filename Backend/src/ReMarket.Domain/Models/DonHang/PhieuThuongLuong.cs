namespace ReMarket.Domain.Models;

public sealed class PhieuThuongLuong : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid CuocTroChuyenId { get; set; }
    public Guid TinNhanId { get; set; }
    public Guid NguoiMuaId { get; set; }
    public Guid NguoiBanId { get; set; }
    public long GiaNiemYet { get; set; }
    public long SoTienGiam { get; set; }
    public string TrangThai { get; set; } = "ChoPhanHoi";
    public DateTimeOffset NgayHetHan { get; set; }
    public CuocTroChuyen CuocTroChuyen { get; set; } = null!;
    public TinNhan TinNhan { get; set; } = null!;
    public ThanhVienTroChuyen NguoiMua { get; set; } = null!;
    public ThanhVienTroChuyen NguoiBan { get; set; } = null!;
}
