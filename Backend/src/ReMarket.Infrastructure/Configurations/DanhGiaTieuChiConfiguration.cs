using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class DanhGiaTieuChiConfiguration : IEntityTypeConfiguration<DanhGiaTieuChi>
{
    public void Configure(EntityTypeBuilder<DanhGiaTieuChi> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("DanhGiaTieuChi", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.HasIndex(x => new { x.DanhGiaId, x.TieuChiDanhGiaId }).IsUnique();
        builder.HasOne(x => x.DanhGia).WithMany()
            .HasForeignKey(x => x.DanhGiaId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.TieuChiDanhGia).WithMany()
            .HasForeignKey(x => x.TieuChiDanhGiaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
