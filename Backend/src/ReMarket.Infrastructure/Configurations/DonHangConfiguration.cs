using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class DonHangConfiguration : IEntityTypeConfiguration<DonHang>
{
    public void Configure(EntityTypeBuilder<DonHang> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("DonHang", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_DonHang_KhacNguoi", "[NguoiMuaId] <> [NguoiBanId]");
            table.HasCheckConstraint("CK_DonHang_TongTien", "[TienHang] > 0 AND [PhiVanChuyen] >= 0 AND [TongTien] = [TienHang] + [PhiVanChuyen]");
            table.HasCheckConstraint("CK_DonHang_TrangThai", "[TrangThai] IN (N'ChoThanhToan', N'DaThanhToan', N'DangGiao', N'DaNhan', N'DaHuy')");
        });
        builder.Property(x => x.TenNguoiNhan).HasMaxLength(120).IsRequired();
        builder.Property(x => x.SoDienThoaiNhan).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DiaChiNhan).HasMaxLength(500).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.HasIndex(x => x.GiaoDichId).IsUnique();
        builder.Property(x => x.BamYeuCau).HasMaxLength(64).IsUnicode(false);
        builder.Property(x => x.HanThanhToan).HasPrecision(0);
        builder.Property(x => x.NgayHuy).HasPrecision(0);
        builder.Property(x => x.LyDoHuy).HasMaxLength(500);
        builder.HasIndex(x => new { x.NguoiMuaId, x.MaYeuCau }).IsUnique().HasFilter("[MaYeuCau] IS NOT NULL");
        builder.HasIndex(x => x.PhieuThuongLuongId).IsUnique().HasFilter("[PhieuThuongLuongId] IS NOT NULL");
        builder.HasIndex(x => new { x.TrangThai, x.HanThanhToan });
        builder.HasIndex(x => new { x.NguoiBanId, x.NgayTao });
        builder.HasOne(x => x.PhieuThuongLuong).WithMany().HasForeignKey(x => x.PhieuThuongLuongId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiMua).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiMuaId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiBan).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiBanId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
