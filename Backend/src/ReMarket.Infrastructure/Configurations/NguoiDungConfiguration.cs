using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("NguoiDung", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_NguoiDung_Email", "LEN(LTRIM(RTRIM([Email]))) > 3");
            table.HasCheckConstraint("CK_NguoiDung_TrangThai", "[TrangThai] IN (N'HoatDong', N'DaKhoa', N'DaXoa')");
        });
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EmailChuanHoa).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EmailChuanHoa).HasComputedColumnSql("UPPER(LTRIM(RTRIM([Email])))", stored: true);
        builder.Property(x => x.SoDienThoai).HasMaxLength(20);
        builder.Property(x => x.MatKhauBam).HasMaxLength(512).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.HasIndex(x => x.EmailChuanHoa).IsUnique();
        builder.HasIndex(x => x.SoDienThoai).IsUnique();
    }
}
