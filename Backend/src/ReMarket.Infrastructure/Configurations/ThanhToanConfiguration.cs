using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ThanhToanConfiguration : IEntityTypeConfiguration<ThanhToan>
{
    public void Configure(EntityTypeBuilder<ThanhToan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.Property(x => x.NgayThanhToan).HasPrecision(0);
        builder.Property(x => x.NgayGiaiNgan).HasPrecision(0);
        builder.Property(x => x.NgayHoanTien).HasPrecision(0);
        builder.ToTable("ThanhToan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_ThanhToan_SoTien", "[SoTien] > 0");
            table.HasCheckConstraint("CK_ThanhToan_TrangThai", "[TrangThai] IN (N'ChoXuLy', N'ThanhCong', N'ThatBai', N'HoanTien')");
        });
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.MaThamChieu).HasMaxLength(100);
        builder.HasIndex(x => x.MaThamChieu).IsUnique();
        builder.HasOne(x => x.DonHang).WithMany()
            .HasForeignKey(x => x.DonHangId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
