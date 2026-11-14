using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class PhienDangNhapConfiguration : IEntityTypeConfiguration<PhienDangNhap>
{
    public void Configure(EntityTypeBuilder<PhienDangNhap> b)
    {
        CauHinhCoBan.ApDung(b);
        b.ToTable("PhienDangNhap", "dbo", t => t.HasCheckConstraint("CK_PhienDangNhap_HetHan", "[NgayHetHan] > [NgayTao]"));
        b.Property(x => x.MaTruyCap).HasMaxLength(32).IsUnicode(false).IsRequired();
        b.Property(x => x.BamRefreshToken).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(x => x.NgayHetHan).HasPrecision(0);
        b.Property(x => x.NgayThuHoi).HasPrecision(0);
        b.HasIndex(x => x.BamRefreshToken).IsUnique();
        b.HasIndex(x => new { x.NguoiDungId, x.NgayThuHoi });
        b.HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.NguoiDungId).OnDelete(DeleteBehavior.NoAction);
    }
}
