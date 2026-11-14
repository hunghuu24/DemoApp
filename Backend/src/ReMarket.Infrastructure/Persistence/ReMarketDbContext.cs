using Microsoft.EntityFrameworkCore;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Persistence;

public sealed class ReMarketDbContext(DbContextOptions<ReMarketDbContext> options) : DbContext(options)
{
    public DbSet<VaiTro> VaiTro => Set<VaiTro>();
    public DbSet<PhienDangNhap> PhienDangNhap => Set<PhienDangNhap>();
    public DbSet<YeuCauDatLaiMatKhau> YeuCauDatLaiMatKhau => Set<YeuCauDatLaiMatKhau>();
    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();
    public DbSet<NguoiDungVaiTro> NguoiDungVaiTro => Set<NguoiDungVaiTro>();
    public DbSet<HoSoCaNhan> HoSoCaNhan => Set<HoSoCaNhan>();
    public DbSet<DanhMuc> DanhMuc => Set<DanhMuc>();
    public DbSet<SanPham> SanPham => Set<SanPham>();
    public DbSet<AnhSanPham> AnhSanPham => Set<AnhSanPham>();
    public DbSet<TinDang> TinDang => Set<TinDang>();
    public DbSet<NhuCauTraoDoi> NhuCauTraoDoi => Set<NhuCauTraoDoi>();
    public DbSet<GiaoDich> GiaoDich => Set<GiaoDich>();
    public DbSet<ThanhVienGiaoDich> ThanhVienGiaoDich => Set<ThanhVienGiaoDich>();
    public DbSet<GiuChoSanPham> GiuChoSanPham => Set<GiuChoSanPham>();
    public DbSet<DonHang> DonHang => Set<DonHang>();
    public DbSet<PhieuThuongLuong> PhieuThuongLuong => Set<PhieuThuongLuong>();
    public DbSet<LichSuHoTro> LichSuHoTro => Set<LichSuHoTro>();
    public DbSet<ChiTietDonHang> ChiTietDonHang => Set<ChiTietDonHang>();
    public DbSet<ThanhToan> ThanhToan => Set<ThanhToan>();
    public DbSet<DeNghiTraoDoi> DeNghiTraoDoi => Set<DeNghiTraoDoi>();
    public DbSet<PhienBanThoaThuan> PhienBanThoaThuan => Set<PhienBanThoaThuan>();
    public DbSet<DongYThoaThuan> DongYThoaThuan => Set<DongYThoaThuan>();
    public DbSet<TienBu> TienBu => Set<TienBu>();
    public DbSet<VongSmartSwap> VongSmartSwap => Set<VongSmartSwap>();
    public DbSet<ThanhVienVong> ThanhVienVong => Set<ThanhVienVong>();
    public DbSet<ChangTraoDoi> ChangTraoDoi => Set<ChangTraoDoi>();
    public DbSet<CuocTroChuyen> CuocTroChuyen => Set<CuocTroChuyen>();
    public DbSet<ThanhVienTroChuyen> ThanhVienTroChuyen => Set<ThanhVienTroChuyen>();
    public DbSet<TinNhan> TinNhan => Set<TinNhan>();
    public DbSet<TepTinNhan> TepTinNhan => Set<TepTinNhan>();
    public DbSet<ThongBao> ThongBao => Set<ThongBao>();
    public DbSet<VanChuyenMoPhong> VanChuyenMoPhong => Set<VanChuyenMoPhong>();
    public DbSet<ThongTinGiaoNhanSmartSwap> ThongTinGiaoNhanSmartSwap => Set<ThongTinGiaoNhanSmartSwap>();
    public DbSet<LichSuVanChuyen> LichSuVanChuyen => Set<LichSuVanChuyen>();
    public DbSet<XacNhanGiaoNhan> XacNhanGiaoNhan => Set<XacNhanGiaoNhan>();
    public DbSet<BangChungGiaoNhan> BangChungGiaoNhan => Set<BangChungGiaoNhan>();
    public DbSet<DanhGia> DanhGia => Set<DanhGia>();
    public DbSet<TieuChiDanhGia> TieuChiDanhGia => Set<TieuChiDanhGia>();
    public DbSet<DanhGiaTieuChi> DanhGiaTieuChi => Set<DanhGiaTieuChi>();
    public DbSet<KhieuNai> KhieuNai => Set<KhieuNai>();
    public DbSet<BangChungKhieuNai> BangChungKhieuNai => Set<BangChungKhieuNai>();
    public DbSet<PhanHoiKhieuNai> PhanHoiKhieuNai => Set<PhanHoiKhieuNai>();
    public DbSet<QuyetDinhXuLy> QuyetDinhXuLy => Set<QuyetDinhXuLy>();
    public DbSet<YeuCauHoTro> YeuCauHoTro => Set<YeuCauHoTro>();
    public DbSet<PhanHoiHoTro> PhanHoiHoTro => Set<PhanHoiHoTro>();
    public DbSet<NhatKyKiemDuyet> NhatKyKiemDuyet => Set<NhatKyKiemDuyet>();
    public DbSet<KhoaTaiKhoan> KhoaTaiKhoan => Set<KhoaTaiKhoan>();
    public DbSet<NhatKyQuanTri> NhatKyQuanTri => Set<NhatKyQuanTri>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_100_CI_AS");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReMarketDbContext).Assembly);
    }
}
