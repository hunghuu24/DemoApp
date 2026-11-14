using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class PhieuThuongLuongConfiguration : IEntityTypeConfiguration<PhieuThuongLuong>
{
    public void Configure(EntityTypeBuilder<PhieuThuongLuong> b)
    {
        CauHinhCoBan.ApDung(b);
        b.ToTable("PhieuThuongLuong", "dbo", t => {
            t.HasCheckConstraint("CK_PhieuThuongLuong_Gia", "[GiaNiemYet]>0 AND [SoTienGiam]>0 AND [SoTienGiam]<[GiaNiemYet]");
            t.HasCheckConstraint("CK_PhieuThuongLuong_Nguoi", "[NguoiMuaId]<>[NguoiBanId]");
            t.HasCheckConstraint("CK_PhieuThuongLuong_Han", "[NgayHetHan]>[NgayTao]");
            t.HasCheckConstraint("CK_PhieuThuongLuong_TrangThai", "[TrangThai] IN ('ChoPhanHoi','DaChapNhan','TuChoi','DaHuy','DaSuDung','HetHan')");
        });
        b.Property(x => x.TrangThai).HasMaxLength(40).IsUnicode(false);
        b.Property(x => x.NgayHetHan).HasPrecision(0);
        b.HasIndex(x => x.TinNhanId).IsUnique();
        b.HasIndex(x => x.CuocTroChuyenId).IsUnique().HasFilter("[TrangThai]='DaChapNhan'");
        b.HasOne(x => x.CuocTroChuyen).WithMany().HasForeignKey(x => x.CuocTroChuyenId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.TinNhan).WithMany().HasForeignKey(x => new { x.TinNhanId, x.CuocTroChuyenId })
            .HasPrincipalKey(x => new { x.Id, x.CuocTroChuyenId }).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.NguoiMua).WithMany().HasForeignKey(x => new { x.CuocTroChuyenId, x.NguoiMuaId })
            .HasPrincipalKey(x => new { x.CuocTroChuyenId, x.NguoiDungId }).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.NguoiBan).WithMany().HasForeignKey(x => new { x.CuocTroChuyenId, x.NguoiBanId })
            .HasPrincipalKey(x => new { x.CuocTroChuyenId, x.NguoiDungId }).OnDelete(DeleteBehavior.NoAction);
    }
}
public sealed class LichSuHoTroConfiguration : IEntityTypeConfiguration<LichSuHoTro>
{
    public void Configure(EntityTypeBuilder<LichSuHoTro> b)
    {
        CauHinhCoBan.ApDung(b);
        b.ToTable("LichSuHoTro", "dbo", t => t.HasCheckConstraint("CK_LichSuHoTro_TrangThai", "[TrangThai] IN ('Moi','DangXuLy','DaGiaiQuyet')"));
        b.Property(x => x.SoThuTu).UseIdentityColumn();
        b.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        b.Property(x => x.NoiDung).HasMaxLength(4000);
        b.HasIndex(x => new { x.YeuCauHoTroId, x.NgayTao });
        b.HasOne(x => x.YeuCauHoTro).WithMany().HasForeignKey(x => x.YeuCauHoTroId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.NguoiThucHien).WithMany().HasForeignKey(x => x.NguoiThucHienId).OnDelete(DeleteBehavior.NoAction);
    }
}
