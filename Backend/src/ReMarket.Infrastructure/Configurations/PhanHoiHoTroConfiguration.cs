using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class PhanHoiHoTroConfiguration : IEntityTypeConfiguration<PhanHoiHoTro>
{
    public void Configure(EntityTypeBuilder<PhanHoiHoTro> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("PhanHoiHoTro", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.NoiDung).HasMaxLength(2000).IsRequired();
        builder.HasOne(x => x.YeuCauHoTro).WithMany()
            .HasForeignKey(x => x.YeuCauHoTroId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiGui).WithMany()
            .HasForeignKey(x => x.NguoiGuiId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
