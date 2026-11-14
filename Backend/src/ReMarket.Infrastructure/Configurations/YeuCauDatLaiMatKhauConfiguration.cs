using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class YeuCauDatLaiMatKhauConfiguration : IEntityTypeConfiguration<YeuCauDatLaiMatKhau>
{
    public void Configure(EntityTypeBuilder<YeuCauDatLaiMatKhau> b)
    {
        CauHinhCoBan.ApDung(b);
        b.ToTable("YeuCauDatLaiMatKhau", "dbo", t => t.HasCheckConstraint("CK_YeuCauDatLaiMatKhau_HetHan", "[NgayHetHan] > [NgayTao]"));
        b.Property(x => x.BamToken).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(x => x.NgayHetHan).HasPrecision(0);
        b.Property(x => x.NgaySuDung).HasPrecision(0);
        b.HasIndex(x => x.BamToken).IsUnique();
        b.HasIndex(x => new { x.NguoiDungId, x.NgayHetHan });
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.NguoiDungId).OnDelete(DeleteBehavior.NoAction);
    }
}
