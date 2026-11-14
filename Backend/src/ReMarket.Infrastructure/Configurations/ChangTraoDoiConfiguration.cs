using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ChangTraoDoiConfiguration : IEntityTypeConfiguration<ChangTraoDoi>
{
    public void Configure(EntityTypeBuilder<ChangTraoDoi> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("ChangTraoDoi", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_ChangTraoDoi_KhacThanhVien", "[ThanhVienGiaoId] <> [ThanhVienNhanId]");
        });
        builder.HasIndex(x => new { x.VongSmartSwapId, x.ThanhVienGiaoId }).IsUnique();
        builder.HasIndex(x => new { x.VongSmartSwapId, x.ThanhVienNhanId }).IsUnique();
        builder.HasOne(x => x.VongSmartSwap).WithMany()
            .HasForeignKey(x => x.VongSmartSwapId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ThanhVienGiao).WithMany()
            .HasForeignKey(x => new { x.ThanhVienGiaoId, x.VongSmartSwapId, x.SanPhamId })
            .HasPrincipalKey(x => new { x.Id, x.VongSmartSwapId, x.SanPhamId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ThanhVienNhan).WithMany()
            .HasForeignKey(x => new { x.ThanhVienNhanId, x.VongSmartSwapId })
            .HasPrincipalKey(x => new { x.Id, x.VongSmartSwapId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
