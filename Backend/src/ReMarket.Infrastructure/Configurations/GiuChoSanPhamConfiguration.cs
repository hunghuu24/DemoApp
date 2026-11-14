using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class GiuChoSanPhamConfiguration : IEntityTypeConfiguration<GiuChoSanPham>
{
    public void Configure(EntityTypeBuilder<GiuChoSanPham> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("GiuChoSanPham", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_GiuChoSanPham_ThoiHan", "[NgayHetHan] > [NgayTao]");
        });
        builder.Property(x => x.NgayHetHan).HasPrecision(0);
        builder.HasIndex(x => x.SanPhamId).IsUnique().HasFilter("[DaGiaiPhong] = 0");
        builder.HasOne(x => x.SanPham).WithMany()
            .HasForeignKey(x => x.SanPhamId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
