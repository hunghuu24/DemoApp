using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ThongBaoConfiguration : IEntityTypeConfiguration<ThongBao>
{
    public void Configure(EntityTypeBuilder<ThongBao> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("ThongBao", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_ThongBao_Loai", "[Loai] IN (N'HeThong', N'TinDang', N'DonHang', N'TraoDoi', N'TinNhan', N'KhieuNai')");
        });
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NoiDung).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.NguoiDungId, x.DaDoc, x.NgayTao });
        builder.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.NguoiDungId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
