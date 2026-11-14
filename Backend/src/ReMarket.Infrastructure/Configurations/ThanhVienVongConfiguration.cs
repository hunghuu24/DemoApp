using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ThanhVienVongConfiguration : IEntityTypeConfiguration<ThanhVienVong>
{
    public void Configure(EntityTypeBuilder<ThanhVienVong> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("ThanhVienVong", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_ThanhVienVong_ViTri", "[ViTri] BETWEEN 1 AND 3");
        });
        builder.HasAlternateKey(x => new { x.Id, x.VongSmartSwapId });
        builder.HasAlternateKey(x => new { x.Id, x.VongSmartSwapId, x.SanPhamId });
        builder.HasIndex(x => new { x.VongSmartSwapId, x.NguoiDungId }).IsUnique();
        builder.HasIndex(x => new { x.VongSmartSwapId, x.SanPhamId }).IsUnique();
        builder.HasIndex(x => new { x.VongSmartSwapId, x.ViTri }).IsUnique();
        builder.HasOne(x => x.TinDang).WithMany().HasForeignKey(x => new { x.TinDangId, x.SanPhamId })
            .HasPrincipalKey(x => new { x.Id, x.SanPhamId }).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.VongSmartSwap).WithMany()
            .HasForeignKey(x => new { x.VongSmartSwapId, x.GiaoDichId })
            .HasPrincipalKey(x => new { x.Id, x.GiaoDichId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ThanhVien).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SanPham).WithMany()
            .HasForeignKey(x => new { x.SanPhamId, x.NguoiDungId })
            .HasPrincipalKey(x => new { x.Id, x.ChuSoHuuId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
