using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class PhanHoiKhieuNaiConfiguration : IEntityTypeConfiguration<PhanHoiKhieuNai>
{
    public void Configure(EntityTypeBuilder<PhanHoiKhieuNai> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("PhanHoiKhieuNai", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.NoiDung).HasMaxLength(2000).IsRequired();
        builder.HasOne(x => x.KhieuNai).WithMany()
            .HasForeignKey(x => x.KhieuNaiId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiGui).WithMany()
            .HasForeignKey(x => x.NguoiGuiId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
