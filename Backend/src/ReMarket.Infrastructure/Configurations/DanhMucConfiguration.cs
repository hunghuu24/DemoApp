using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class DanhMucConfiguration : IEntityTypeConfiguration<DanhMuc>
{
    public void Configure(EntityTypeBuilder<DanhMuc> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("DanhMuc", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_DanhMuc_KhongTuThamChieu", "[DanhMucChaId] IS NULL OR [DanhMucChaId] <> [Id]");
            table.HasCheckConstraint("CK_DanhMuc_ThuTu", "[ThuTu] >= 0");
        });
        builder.Property(x => x.Ten).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Ma).HasMaxLength(60).IsRequired();
        builder.Property(x => x.BieuTuong).HasMaxLength(30).HasDefaultValue("");
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne(x => x.DanhMucCha).WithMany()
            .HasForeignKey(x => x.DanhMucChaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
