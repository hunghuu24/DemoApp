using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class TieuChiDanhGiaConfiguration : IEntityTypeConfiguration<TieuChiDanhGia>
{
    public void Configure(EntityTypeBuilder<TieuChiDanhGia> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("TieuChiDanhGia", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.Ma).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
    }
}
