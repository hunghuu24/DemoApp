using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ThanhVienGiaoDichConfiguration : IEntityTypeConfiguration<ThanhVienGiaoDich>
{
    public void Configure(EntityTypeBuilder<ThanhVienGiaoDich> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("ThanhVienGiaoDich", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.VaiTroHienThi).HasMaxLength(40).IsRequired();
        builder.HasAlternateKey(x => new { x.GiaoDichId, x.NguoiDungId });
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.NguoiDungId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
