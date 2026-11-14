using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class PhienBanThoaThuanConfiguration : IEntityTypeConfiguration<PhienBanThoaThuan>
{
    public void Configure(EntityTypeBuilder<PhienBanThoaThuan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("PhienBanThoaThuan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_PhienBanThoaThuan_PhienBan", "[SoPhienBan] > 0");
            table.HasCheckConstraint("CK_PhienBanThoaThuan_TrangThai", "[TrangThai] IN (N'DeXuat', N'DaChot', N'DaThayThe')");
        });
        builder.Property(x => x.NoiDung).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.DiaDiemHen).HasMaxLength(500);
        builder.Property(x => x.ThoiGianHen).HasPrecision(0);
        builder.Property(x => x.BamYeuCau).HasMaxLength(64).IsUnicode(false);
        builder.HasIndex(x => new { x.GiaoDichId, x.NguoiTaoId, x.MaYeuCau }).IsUnique().HasFilter("[MaYeuCau] IS NOT NULL");
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.HasAlternateKey(x => new { x.Id, x.GiaoDichId });
        builder.HasIndex(x => new { x.GiaoDichId, x.SoPhienBan }).IsUnique();
        builder.HasIndex(x => x.GiaoDichId).IsUnique().HasFilter("[TrangThai] = 'DaChot'");
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiTao).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiTaoId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
