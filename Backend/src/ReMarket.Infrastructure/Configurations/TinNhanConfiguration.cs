using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class TinNhanConfiguration : IEntityTypeConfiguration<TinNhan>
{
    public void Configure(EntityTypeBuilder<TinNhan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.HasOne(x => x.PhienBanThoaThuan).WithMany().HasForeignKey(x => x.PhienBanThoaThuanId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.PhienBanThoaThuanId).IsUnique().HasFilter("[PhienBanThoaThuanId] IS NOT NULL");
        builder.Property(x => x.SoThuTu).UseIdentityColumn();
        builder.HasIndex(x => new { x.CuocTroChuyenId, x.SoThuTu });
        builder.HasAlternateKey(x => new { x.Id, x.CuocTroChuyenId });
        builder.ToTable("TinNhan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_TinNhan_VanBan", "[Loai] <> N'VanBan' OR ([NoiDung] IS NOT NULL AND LEN(LTRIM(RTRIM([NoiDung]))) > 0)");
            table.HasCheckConstraint("CK_TinNhan_Loai", "[Loai] IN (N'VanBan', N'HinhAnh', N'HeThong', N'DeNghiGia')");
        });
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.NoiDung).HasMaxLength(4000);
        builder.HasIndex(x => new { x.CuocTroChuyenId, x.NguoiGuiId, x.MaPhiaClient }).IsUnique();
        builder.HasIndex(x => new { x.CuocTroChuyenId, x.NgayTao });
        builder.HasOne(x => x.NguoiGui).WithMany()
            .HasForeignKey(x => new { x.CuocTroChuyenId, x.NguoiGuiId })
            .HasPrincipalKey(x => new { x.CuocTroChuyenId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
