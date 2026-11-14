using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class NhatKyKiemDuyetConfiguration : IEntityTypeConfiguration<NhatKyKiemDuyet>
{
    public void Configure(EntityTypeBuilder<NhatKyKiemDuyet> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("NhatKyKiemDuyet", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_NhatKyKiemDuyet_TrangThaiTruoc", "[TrangThaiTruoc] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')");
            table.HasCheckConstraint("CK_NhatKyKiemDuyet_TrangThaiSau", "[TrangThaiSau] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')");
        });
        builder.Property(x => x.TrangThaiTruoc).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.TrangThaiSau).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.LyDo).HasMaxLength(1000).IsRequired();
        builder.HasOne(x => x.TinDang).WithMany()
            .HasForeignKey(x => x.TinDangId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.QuanTriVien).WithMany()
            .HasForeignKey(x => x.QuanTriVienId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
