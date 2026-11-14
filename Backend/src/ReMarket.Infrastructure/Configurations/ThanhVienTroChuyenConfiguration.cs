using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ThanhVienTroChuyenConfiguration : IEntityTypeConfiguration<ThanhVienTroChuyen>
{
    public void Configure(EntityTypeBuilder<ThanhVienTroChuyen> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("ThanhVienTroChuyen", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.LanDocCuoi).HasPrecision(0);
        builder.HasAlternateKey(x => new { x.CuocTroChuyenId, x.NguoiDungId });
        builder.HasOne(x => x.CuocTroChuyen).WithMany()
            .HasForeignKey(x => x.CuocTroChuyenId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.NguoiDungId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
