using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class TienBuConfiguration : IEntityTypeConfiguration<TienBu>
{
    public void Configure(EntityTypeBuilder<TienBu> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("TienBu", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_TienBu_SoTienVaNguoi", "[SoTien] > 0 AND [NguoiTraId] <> [NguoiNhanId]");
        });
        builder.HasIndex(x => new { x.PhienBanThoaThuanId, x.NguoiTraId, x.NguoiNhanId }).IsUnique();
        builder.HasOne(x => x.PhienBanThoaThuan).WithMany()
            .HasForeignKey(x => new { x.PhienBanThoaThuanId, x.GiaoDichId })
            .HasPrincipalKey(x => new { x.Id, x.GiaoDichId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiTra).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiTraId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiNhan).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiNhanId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
