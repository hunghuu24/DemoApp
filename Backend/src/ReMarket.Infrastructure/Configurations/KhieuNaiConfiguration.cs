using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class KhieuNaiConfiguration : IEntityTypeConfiguration<KhieuNai>
{
    public void Configure(EntityTypeBuilder<KhieuNai> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("KhieuNai", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_KhieuNai_KhacNguoi", "[NguoiGuiId] <> [NguoiBiKhieuNaiId]");
            table.HasCheckConstraint("CK_KhieuNai_SuCoSmartSwap", "([VanChuyenMoPhongId] IS NULL AND [LoaiSuCo] IS NULL) OR ([VanChuyenMoPhongId] IS NOT NULL AND [LoaiSuCo] IN ('SaiHang','ChuaNhanHang'))");
            table.HasCheckConstraint("CK_KhieuNai_TrangThai", "[TrangThai] IN (N'Moi', N'DangXuLy', N'ChoThucHien', N'DaGiaiQuyet', N'TuChoi')");
        });
        builder.Property(x => x.Ma).HasMaxLength(30).IsRequired();
        builder.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NoiDung).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.HanPhanHoi).HasPrecision(0);
        builder.Property(x => x.BamYeuCau).HasMaxLength(64).IsUnicode(false);
        builder.Property(x => x.LoaiSuCo).HasMaxLength(40).IsUnicode(false);
        builder.HasOne(x => x.VanChuyenMoPhong).WithMany().HasForeignKey(x => x.VanChuyenMoPhongId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => new { x.NguoiGuiId,x.MaYeuCau }).IsUnique().HasFilter("[MaYeuCau] IS NOT NULL");
        builder.HasIndex(x => x.Ma).IsUnique();
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiGui).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiGuiId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiBiKhieuNai).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiBiKhieuNaiId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
