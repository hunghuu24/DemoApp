using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class BangChungGiaoNhanConfiguration : IEntityTypeConfiguration<BangChungGiaoNhan>
{
    public void Configure(EntityTypeBuilder<BangChungGiaoNhan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("BangChungGiaoNhan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.DuongDan).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.LoaiMime).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BamNoiDung).HasMaxLength(64).IsUnicode(false);
        builder.HasIndex(x => new { x.XacNhanGiaoNhanId, x.MaYeuCau }).IsUnique().HasFilter("[MaYeuCau] IS NOT NULL");
        builder.HasOne(x => x.XacNhanGiaoNhan).WithMany()
            .HasForeignKey(x => x.XacNhanGiaoNhanId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiTaiLen).WithMany()
            .HasForeignKey(x => x.NguoiTaiLenId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
