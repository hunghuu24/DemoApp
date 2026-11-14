using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class TepTinNhanConfiguration : IEntityTypeConfiguration<TepTinNhan>
{
    public void Configure(EntityTypeBuilder<TepTinNhan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("TepTinNhan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_TepTinNhan_DungLuong", "[DungLuong] > 0");
        });
        builder.Property(x => x.DuongDan).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.TenTep).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LoaiMime).HasMaxLength(100).IsRequired();
        builder.HasOne(x => x.TinNhan).WithMany()
            .HasForeignKey(x => x.TinNhanId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
