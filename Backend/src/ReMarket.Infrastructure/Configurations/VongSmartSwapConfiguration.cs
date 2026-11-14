using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class VongSmartSwapConfiguration : IEntityTypeConfiguration<VongSmartSwap>
{
    public void Configure(EntityTypeBuilder<VongSmartSwap> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("VongSmartSwap", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_VongSmartSwap_ThoiHan", "[NgayHetHan] > [NgayTao]");
            table.HasCheckConstraint("CK_VongSmartSwap_KetThuc", "[MaKetThuc] IS NULL OR ([TrangThai]='DaHuy' AND [MaKetThuc] IN ('TuChoi','HetHan','DaHuy'))");
            table.HasCheckConstraint("CK_VongSmartSwap_TrangThai", "[TrangThai] IN (N'ChoDongY', N'SanSangBanGiao', N'DangBanGiao', N'HoanTat', N'DaHuy', N'TamDung')");
        });
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.NgayHetHan).HasPrecision(0);
        builder.Property(x => x.NgayChapNhan).HasPrecision(0);
        builder.Property(x => x.BamYeuCau).HasMaxLength(64).IsUnicode(false);
        builder.Property(x => x.MaDeXuat).HasMaxLength(98).IsUnicode(false);
        builder.Property(x => x.MaKetThuc).HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.LyDoKetThuc).HasMaxLength(500);
        builder.HasIndex(x => new { x.NguoiKhoiTaoId, x.MaYeuCau }).IsUnique().HasFilter("[NguoiKhoiTaoId] IS NOT NULL AND [MaYeuCau] IS NOT NULL");
        builder.HasIndex(x => x.MaDeXuat).IsUnique().HasFilter("[MaDeXuat] IS NOT NULL AND [TrangThai]='ChoDongY'");
        builder.HasIndex(x => new { x.TrangThai, x.NgayHetHan });
        builder.HasOne<ThanhVienGiaoDich>().WithMany().HasForeignKey(x => new { x.GiaoDichId, x.NguoiKhoiTaoId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId }).OnDelete(DeleteBehavior.NoAction);
        builder.HasAlternateKey(x => new { x.Id, x.GiaoDichId });
        builder.HasIndex(x => x.GiaoDichId).IsUnique();
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
