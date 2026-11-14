using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class QuyetDinhXuLyConfiguration : IEntityTypeConfiguration<QuyetDinhXuLy>
{
    public void Configure(EntityTypeBuilder<QuyetDinhXuLy> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("QuyetDinhXuLy", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.KetLuan).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.BienPhap).HasMaxLength(2000).IsRequired();
        builder.Property(x=>x.CanCu).HasMaxLength(2000).HasDefaultValue("");
        builder.Property(x=>x.ThongBao).HasMaxLength(2000).HasDefaultValue("");
        builder.Property(x=>x.HuongXuLy).HasMaxLength(20).HasDefaultValue("GhiNhan");
        builder.Property(x=>x.BamYeuCau).HasMaxLength(64);
        builder.HasIndex(x=>new{x.KhieuNaiId,x.MaYeuCau}).IsUnique().HasFilter("[MaYeuCau] IS NOT NULL");
        builder.HasOne(x => x.KhieuNai).WithMany()
            .HasForeignKey(x => x.KhieuNaiId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.QuanTriVien).WithMany()
            .HasForeignKey(x => x.QuanTriVienId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
