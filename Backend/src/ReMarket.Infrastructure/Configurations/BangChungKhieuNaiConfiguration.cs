using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class BangChungKhieuNaiConfiguration : IEntityTypeConfiguration<BangChungKhieuNai>
{
    public void Configure(EntityTypeBuilder<BangChungKhieuNai> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("BangChungKhieuNai", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
        });
        builder.Property(x => x.DuongDan).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.LoaiMime).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MoTa).HasMaxLength(1000).IsRequired();
        builder.HasOne(x => x.KhieuNai).WithMany()
            .HasForeignKey(x => x.KhieuNaiId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiTaiLen).WithMany()
            .HasForeignKey(x => x.NguoiTaiLenId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
