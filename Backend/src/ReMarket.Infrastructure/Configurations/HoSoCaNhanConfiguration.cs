using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class HoSoCaNhanConfiguration : IEntityTypeConfiguration<HoSoCaNhan>
{
    public void Configure(EntityTypeBuilder<HoSoCaNhan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("HoSoCaNhan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.HoTen).HasMaxLength(120).IsRequired();
        builder.Property(x => x.AnhDaiDienUrl).HasMaxLength(1000);
        builder.Property(x => x.DiaChi).HasMaxLength(500);
        builder.Property(x => x.TinhThanh).HasMaxLength(100);
        builder.HasIndex(x => x.NguoiDungId).IsUnique();
        builder.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.NguoiDungId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
