using System.ComponentModel.DataAnnotations;
using ReMarket.Domain.Enums;

namespace ReMarket.Application.SmartSwap;

public sealed class TimSmartSwapRequest
{
    public Guid? TinDangId { get; set; }
    [MaxLength(100)] public string? TinhThanh { get; set; }
    [MaxLength(100)] public string? TuKhoaMuonNhan { get; set; }
    [Range(0,1_000_000_000_000)] public long? BuToiDa { get; set; }
    public TinhTrangSanPham? TinhTrangToiThieu { get; set; }
    public bool ChiThanhVienDaXacMinh { get; set; }
    [Range(typeof(decimal),"0","100")] public decimal DiemToiThieu { get; set; }
    [Range(1,1000)] public int Trang { get; set; } = 1;
    [Range(1,50)] public int KichThuocTrang { get; set; } = 20;
}

// Projection from active SQL listings; never from pre-existing VongSmartSwap rows.
public sealed record UngVienSmartSwap(Guid TinDangId, Guid SanPhamId, Guid NguoiDungId,
    string TenSanPham, Guid DanhMucId, long GiaTriThamKhao, TinhTrangSanPham TinhTrang,
    string TinhThanh, string PhienBan, string MoTaMongMuon, Guid? DanhMucMongMuonId,
    string? TuKhoaMongMuon, TinhTrangSanPham? TinhTrangToiThieu,
    long? GiaTriMongMuonTu, long? GiaTriMongMuonDen, bool ChoPhepBuTien, long SoTienBuToiDa);
public sealed record DanhMucSmartSwap(Guid Id, Guid? ChaId, string Ten);
public sealed record MonSmartSwapDto(Guid TinDangId, Guid SanPhamId, Guid NguoiDungId,
    string Ten, long GiaTriThamKhao, string TinhTrang, string TinhThanh, string PhienBan);
public sealed record ChangSmartSwapDto(Guid NguoiGiaoId, Guid NguoiNhanId, MonSmartSwapDto SanPham,
    decimal DiemPhuHop, List<string> LyDo);
public sealed record CanDoiSmartSwapDto(Guid NguoiDungId, Guid TinChoId, Guid TinNhanId,
    long GiaTriCho, long GiaTriNhan, long TienCanBu, long TienDuocNhan, long MucBuToiDa);
public sealed record KhoanBuSmartSwapDto(Guid NguoiTraId, Guid NguoiNhanId, long SoTien);
public sealed record VongSmartSwapDeXuatDto(string MaDeXuat, decimal DiemPhuHop, long TongTienBu,
    List<ChangSmartSwapDto> Chang, List<CanDoiSmartSwapDto> ThanhVien, List<KhoanBuSmartSwapDto> TienBu);
public sealed record KetQuaTimSmartSwapDto(List<VongSmartSwapDeXuatDto> Items, int TongSo, int Trang,
    int KichThuocTrang, int TongTrang, int SoTinKhaDung, int SoCanh, DateTimeOffset ThoiDiemTim, string ThongBao,
    List<HoSoThanhVienSmartSwapDto>? HoSo = null);

public sealed class GioiHanTimSmartSwap
{
    public int SoTinToiDa { get; set; } = 1500;
    public int SoCanhToiDa { get; set; } = 250000;
    public int SoLuotDuyetToiDa { get; set; } = 2000000;
}

// Explicit resource-limit error: never return an incomplete result disguised as "not found".
public sealed class VuotGioiHanSmartSwapException : Exception
{
    public VuotGioiHanSmartSwapException() : base("Có quá nhiều ứng viên Smart Swap. Hãy lọc tỉnh/thành phố để thu hẹp tìm kiếm.") { }
}
