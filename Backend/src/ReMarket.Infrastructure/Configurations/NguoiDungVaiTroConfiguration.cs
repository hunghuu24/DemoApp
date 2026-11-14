using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class NguoiDungVaiTroConfiguration : IEntityTypeConfiguration<NguoiDungVaiTro>
{
    public void Configure(EntityTypeBuilder<NguoiDungVaiTro> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("NguoiDungVaiTro", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.HasIndex(x => new { x.NguoiDungId, x.VaiTroId }).IsUnique();
        builder.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.NguoiDungId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.VaiTro).WithMany()
            .HasForeignKey(x => x.VaiTroId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
