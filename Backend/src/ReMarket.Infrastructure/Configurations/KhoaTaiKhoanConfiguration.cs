using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class KhoaTaiKhoanConfiguration : IEntityTypeConfiguration<KhoaTaiKhoan>
{
    public void Configure(EntityTypeBuilder<KhoaTaiKhoan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("KhoaTaiKhoan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_KhoaTaiKhoan_ThoiHan", "([NgayKetThuc] IS NULL OR [NgayKetThuc] > [NgayBatDau]) AND ([NgayMoKhoa] IS NULL OR [NgayMoKhoa] >= [NgayBatDau])");
            table.HasCheckConstraint("CK_KhoaTaiKhoan_KhongTuKhoa", "[NguoiDungId] <> [QuanTriVienId]");
        });
        builder.Property(x => x.LyDo).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.NgayBatDau).HasPrecision(0);
        builder.Property(x => x.NgayKetThuc).HasPrecision(0);
        builder.Property(x => x.NgayMoKhoa).HasPrecision(0);
        builder.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.NguoiDungId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.QuanTriVien).WithMany()
            .HasForeignKey(x => x.QuanTriVienId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
