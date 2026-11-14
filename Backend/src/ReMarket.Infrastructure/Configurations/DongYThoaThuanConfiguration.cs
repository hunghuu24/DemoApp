using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class DongYThoaThuanConfiguration : IEntityTypeConfiguration<DongYThoaThuan>
{
    public void Configure(EntityTypeBuilder<DongYThoaThuan> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("DongYThoaThuan", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_DongYThoaThuan_NgayPhanHoi", "([QuyetDinh] = N'ChoPhanHoi' AND [NgayPhanHoi] IS NULL) OR ([QuyetDinh] <> N'ChoPhanHoi' AND [NgayPhanHoi] IS NOT NULL)");
            table.HasCheckConstraint("CK_DongYThoaThuan_QuyetDinh", "[QuyetDinh] IN (N'ChoPhanHoi', N'DongY', N'TuChoi')");
        });
        builder.Property(x => x.QuyetDinh).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.NgayPhanHoi).HasPrecision(0);
        builder.HasIndex(x => new { x.PhienBanThoaThuanId, x.NguoiDungId }).IsUnique();
        builder.HasOne(x => x.PhienBanThoaThuan).WithMany()
            .HasForeignKey(x => new { x.PhienBanThoaThuanId, x.GiaoDichId })
            .HasPrincipalKey(x => new { x.Id, x.GiaoDichId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ThanhVien).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
