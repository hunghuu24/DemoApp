using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class VanChuyenMoPhongConfiguration : IEntityTypeConfiguration<VanChuyenMoPhong>
{
    public void Configure(EntityTypeBuilder<VanChuyenMoPhong> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("VanChuyenMoPhong", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_VanChuyenMoPhong_TrangThai", "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai', N'ChoGui', N'DaGui')");
            table.HasCheckConstraint("CK_VanChuyenMoPhong_Nguon", "([DonHangId] IS NOT NULL AND [ChangTraoDoiId] IS NULL AND [PhienBanThoaThuanId] IS NULL) OR ([DonHangId] IS NULL AND [ChangTraoDoiId] IS NOT NULL AND [PhienBanThoaThuanId] IS NOT NULL)");
            table.HasCheckConstraint("CK_VanChuyenMoPhong_SmartSwap", "[ChangTraoDoiId] IS NULL OR ([TenNguoiGui] IS NOT NULL AND [DienThoaiGui] IS NOT NULL AND [DiaChiGui] IS NOT NULL AND [TenNguoiNhan] IS NOT NULL AND [DienThoaiNhan] IS NOT NULL AND [DiaChiNhan] IS NOT NULL AND [TrangThai] IN ('ChoGui','DaGui','DangVanChuyen','DaGiao') AND ([TrangThai]='ChoGui' OR [MaVanDonTuNhap] IS NOT NULL) AND ([NgayNhanDungHang] IS NULL OR [TrangThai]='DaGiao'))");
        });
        builder.Property(x => x.MaVanDon).HasMaxLength(40).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.NgayGiaoDuKien).HasPrecision(0);
        builder.Property(x => x.MaVanDonTuNhap).HasMaxLength(40);
        builder.Property(x => x.NgayNhanDungHang).HasPrecision(0);
        builder.Property(x => x.TenNguoiGui).HasMaxLength(120);
        builder.Property(x => x.TenNguoiNhan).HasMaxLength(120);
        builder.Property(x => x.DienThoaiGui).HasMaxLength(10);
        builder.Property(x => x.DienThoaiNhan).HasMaxLength(10);
        builder.Property(x => x.DiaChiGui).HasMaxLength(500);
        builder.Property(x => x.DiaChiNhan).HasMaxLength(500);
        builder.HasIndex(x => x.DonHangId).IsUnique().HasFilter("[DonHangId] IS NOT NULL");
        builder.HasIndex(x => x.ChangTraoDoiId).IsUnique().HasFilter("[ChangTraoDoiId] IS NOT NULL");
        builder.HasIndex(x => x.MaVanDonTuNhap).IsUnique().HasFilter("[MaVanDonTuNhap] IS NOT NULL");
        builder.HasOne(x => x.ChangTraoDoi).WithMany().HasForeignKey(x => x.ChangTraoDoiId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.PhienBanThoaThuan).WithMany().HasForeignKey(x => x.PhienBanThoaThuanId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.MaVanDon).IsUnique();
        builder.HasOne(x => x.DonHang).WithMany()
            .HasForeignKey(x => x.DonHangId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
