using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class AnhSanPhamConfiguration : IEntityTypeConfiguration<AnhSanPham>
{
    public void Configure(EntityTypeBuilder<AnhSanPham> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("AnhSanPham", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_AnhSanPham_ThuTu", "[ThuTu] >= 0");
        });
        builder.Property(x => x.DuongDan).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => new { x.SanPhamId, x.ThuTu }).IsUnique();
        builder.HasIndex(x => x.SanPhamId).IsUnique().HasFilter("[LaAnhBia] = 1");
        builder.HasOne(x => x.SanPham).WithMany()
            .HasForeignKey(x => x.SanPhamId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
