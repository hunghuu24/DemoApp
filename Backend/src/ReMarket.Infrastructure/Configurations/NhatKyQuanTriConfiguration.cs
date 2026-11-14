using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class NhatKyQuanTriConfiguration : IEntityTypeConfiguration<NhatKyQuanTri>
{
    public void Configure(EntityTypeBuilder<NhatKyQuanTri> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("NhatKyQuanTri", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_NhatKyQuanTri_Json", "ISJSON([ChiTietJson]) = 1");
        });
        builder.Property(x => x.HanhDong).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LoaiDoiTuong).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChiTietJson).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => new { x.LoaiDoiTuong, x.DoiTuongId, x.NgayTao });
        builder.HasOne(x => x.NguoiThucHien).WithMany()
            .HasForeignKey(x => x.NguoiThucHienId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
