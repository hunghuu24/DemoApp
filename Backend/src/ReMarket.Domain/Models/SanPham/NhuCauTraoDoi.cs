using ReMarket.Domain.Enums;

namespace ReMarket.Domain.Models;

public sealed class NhuCauTraoDoi : IThucThe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.UtcNow;
    public byte[] PhienBan { get; set; } = [];
    public Guid TinDangId { get; set; }
    public string MoTaMongMuon { get; set; } = string.Empty;
    public Guid? DanhMucMongMuonId { get; set; }
    public string? TuKhoaMongMuon { get; set; }
    public TinhTrangSanPham? TinhTrangToiThieu { get; set; }
    public long? GiaTriMongMuonTu { get; set; }
    public long? GiaTriMongMuonDen { get; set; }
    public bool ChoPhepBuTien { get; set; }
    public long SoTienBuToiDa { get; set; }
    public bool ChoPhepSmartSwap { get; set; }
    public bool ChoPhepMotMot { get; set; } = true;
    public string? GhiChu { get; set; }

    public TinDang TinDang { get; set; } = null!;
    public DanhMuc? DanhMucMongMuon { get; set; }
}
