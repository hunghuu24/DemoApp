using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class VaiTroConfiguration : IEntityTypeConfiguration<VaiTro>
{
    public void Configure(EntityTypeBuilder<VaiTro> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("VaiTro", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.Ma).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Ten).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Ma).IsUnique();
    }
}
