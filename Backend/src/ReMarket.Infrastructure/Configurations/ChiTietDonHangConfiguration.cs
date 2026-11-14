using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ChiTietDonHangConfiguration : IEntityTypeConfiguration<ChiTietDonHang>
{
    public void Configure(EntityTypeBuilder<ChiTietDonHang> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("ChiTietDonHang", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_ChiTietDonHang_GiaSoLuong", "[DonGia] > 0 AND [SoLuong] = 1");
            table.HasCheckConstraint("CK_ChiTietDonHang_GiamGia", "[SoTienGiam]>=0 AND ([GiaNiemYet] IS NULL OR ([GiaNiemYet]>0 AND [DonGia]=[GiaNiemYet]-[SoTienGiam]))");
        });
        builder.Property(x => x.TenSanPham).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.DonHangId, x.SanPhamId }).IsUnique();
        builder.HasOne(x => x.DonHang).WithMany()
            .HasForeignKey(x => x.DonHangId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.TinDang).WithMany()
            .HasForeignKey(x => new { x.TinDangId, x.SanPhamId })
            .HasPrincipalKey(x => new { x.Id, x.SanPhamId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
