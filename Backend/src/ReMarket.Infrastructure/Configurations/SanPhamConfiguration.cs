using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class SanPhamConfiguration : IEntityTypeConfiguration<SanPham>
{
    public void Configure(EntityTypeBuilder<SanPham> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("SanPham", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_SanPham_GiaTri", "[GiaTriThamKhao] >= 0");
            table.HasCheckConstraint("CK_SanPham_TinhTrang", "[TinhTrang] IN (N'NhuMoi', N'DaSuDungTot', N'CoLoiNhe')");
        });
        builder.Property(x => x.Ten).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MoTa).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.TinhTrang).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.HasAlternateKey(x => new { x.Id, x.ChuSoHuuId });
        builder.HasOne(x => x.ChuSoHuu).WithMany()
            .HasForeignKey(x => x.ChuSoHuuId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DanhMuc).WithMany()
            .HasForeignKey(x => x.DanhMucId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
