using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class ThongTinGiaoNhanSmartSwapConfiguration:IEntityTypeConfiguration<ThongTinGiaoNhanSmartSwap>
{
    public void Configure(EntityTypeBuilder<ThongTinGiaoNhanSmartSwap> b)
    {
        CauHinhCoBan.ApDung(b);b.ToTable("ThongTinGiaoNhanSmartSwap",t=>t.UseSqlOutputClause(false));
        b.Property(x=>x.TenLienHe).HasMaxLength(120).IsRequired();b.Property(x=>x.SoDienThoai).HasMaxLength(10).IsRequired();
        b.Property(x=>x.DiaChiGui).HasMaxLength(500).IsRequired();b.Property(x=>x.DiaChiNhan).HasMaxLength(500).IsRequired();
        b.Property(x=>x.BamYeuCau).HasMaxLength(64).IsUnicode(false).IsRequired();b.Property(x=>x.NgayXacNhan).HasPrecision(0);
        b.HasIndex(x=>x.ThanhVienVongId).IsUnique();
        b.HasOne(x=>x.ThanhVienVong).WithMany().HasForeignKey(x=>x.ThanhVienVongId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x=>x.PhienBanThoaThuan).WithMany().HasForeignKey(x=>x.PhienBanThoaThuanId).OnDelete(DeleteBehavior.NoAction);
    }
}
