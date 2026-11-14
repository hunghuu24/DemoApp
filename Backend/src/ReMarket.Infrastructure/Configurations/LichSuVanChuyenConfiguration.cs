using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class LichSuVanChuyenConfiguration : IEntityTypeConfiguration<LichSuVanChuyen>
{
    public void Configure(EntityTypeBuilder<LichSuVanChuyen> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.Property(x => x.SoThuTu).UseIdentityColumn();
        builder.ToTable("LichSuVanChuyen", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_LichSuVanChuyen_TrangThai", "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai', N'ChoGui', N'DaGui')");
        });
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.MoTa).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ThoiDiem).HasPrecision(0);
        builder.HasIndex(x => new { x.VanChuyenMoPhongId, x.ThoiDiem });
        builder.HasOne(x => x.VanChuyenMoPhong).WithMany()
            .HasForeignKey(x => x.VanChuyenMoPhongId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
