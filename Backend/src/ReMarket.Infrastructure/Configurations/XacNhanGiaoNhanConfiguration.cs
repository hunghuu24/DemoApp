using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class XacNhanGiaoNhanConfiguration : IEntityTypeConfiguration<XacNhanGiaoNhan>
{
    public void Configure(EntityTypeBuilder<XacNhanGiaoNhan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("XacNhanGiaoNhan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_XacNhanGiaoNhan_KhacNguoi", "[NguoiXacNhanId] <> [NguoiDoiTacId]");
            table.HasCheckConstraint("CK_XacNhanGiaoNhan_Loai", "[Loai] IN (N'DaGiao', N'DaNhan')");
        });
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.NgayXacNhan).HasPrecision(0);
        builder.HasIndex(x => new { x.GiaoDichId, x.NguoiXacNhanId, x.Loai }).IsUnique();
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiXacNhan).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiXacNhanId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiDoiTac).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiDoiTacId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SanPham).WithMany()
            .HasForeignKey(x => x.SanPhamId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.PhienBanThoaThuan).WithMany()
            .HasForeignKey(x => new { x.PhienBanThoaThuanId, x.GiaoDichId })
            .HasPrincipalKey(x => new { x.Id, x.GiaoDichId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
