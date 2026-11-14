using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class GiaoDichConfiguration : IEntityTypeConfiguration<GiaoDich>
{
    public void Configure(EntityTypeBuilder<GiaoDich> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("GiaoDich", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_GiaoDich_NgayHoanTat", "([TrangThai] = N'HoanTat' AND [NgayHoanTat] IS NOT NULL) OR ([TrangThai] <> N'HoanTat' AND [NgayHoanTat] IS NULL)");
            table.HasCheckConstraint("CK_GiaoDich_Loai", "[Loai] IN (N'MuaBan', N'TraoDoiMotMot', N'SmartSwap')");
            table.HasCheckConstraint("CK_GiaoDich_TrangThai", "[TrangThai] IN (N'KhoiTao', N'ChoDongY', N'DangThucHien', N'HoanTat', N'DaHuy', N'TranhChap')");
        });
        builder.Property(x => x.Ma).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.NgayHoanTat).HasPrecision(0);
        builder.HasIndex(x => x.Ma).IsUnique();
    }
}
