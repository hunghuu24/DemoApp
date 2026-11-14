using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class YeuCauHoTroConfiguration : IEntityTypeConfiguration<YeuCauHoTro>
{
    public void Configure(EntityTypeBuilder<YeuCauHoTro> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("YeuCauHoTro", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_YeuCauHoTro_Loai", "[Loai] IN (N'DonHangVanChuyen', N'ThanhToanHoanTien', N'TaiKhoanKhac')");
            table.HasCheckConstraint("CK_YeuCauHoTro_TrangThai", "[TrangThai] IN (N'Moi', N'DangXuLy', N'DaGiaiQuyet')");
        });
        builder.Property(x => x.Ma).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NoiDung).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasIndex(x => new { x.NguoiGuiId, x.MaYeuCau }).IsUnique().HasFilter("[MaYeuCau] IS NOT NULL");
        builder.HasOne(x => x.NguoiGui).WithMany()
            .HasForeignKey(x => x.NguoiGuiId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DonHang).WithMany()
            .HasForeignKey(x => x.DonHangId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
