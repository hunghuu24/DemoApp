using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class DanhGiaConfiguration : IEntityTypeConfiguration<DanhGia>
{
    public void Configure(EntityTypeBuilder<DanhGia> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("DanhGia", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_DanhGia_SoSaoVaNguoi", "[SoSao] BETWEEN 1 AND 5 AND [NguoiDanhGiaId] <> [NguoiDuocDanhGiaId]");
            table.HasCheckConstraint("CK_DanhGia_SanPham", "[SoSaoSanPham] IS NULL OR [SoSaoSanPham] BETWEEN 1 AND 5");
        });
        builder.Property(x => x.NoiDung).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.GiaoDichId, x.NguoiDanhGiaId, x.NguoiDuocDanhGiaId }).IsUnique();
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiDanhGia).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiDanhGiaId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiDuocDanhGia).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiDuocDanhGiaId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
