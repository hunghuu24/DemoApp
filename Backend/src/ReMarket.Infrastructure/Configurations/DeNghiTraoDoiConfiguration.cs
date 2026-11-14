using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class DeNghiTraoDoiConfiguration : IEntityTypeConfiguration<DeNghiTraoDoi>
{
    public void Configure(EntityTypeBuilder<DeNghiTraoDoi> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("DeNghiTraoDoi", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_DeNghiTraoDoi_KhacNguoiVaMon", "[NguoiGuiId] <> [NguoiNhanId] AND [SanPhamGuiId] <> [SanPhamNhanId]");
            table.HasCheckConstraint("CK_DeNghiTraoDoi_ThoiHan", "[NgayHetHan] > [NgayTao]");
            table.HasCheckConstraint("CK_DeNghiTraoDoi_Tin", "([TinGuiId] IS NULL AND [TinNhanId] IS NULL) OR ([TinGuiId] IS NOT NULL AND [TinNhanId] IS NOT NULL AND [TinGuiId]<>[TinNhanId])");
            table.HasCheckConstraint("CK_DeNghiTraoDoi_TrangThai", "[TrangThai] IN (N'ChoPhanHoi', N'DeXuatLai', N'DaChapNhan', N'TuChoi', N'DaHuy', N'HetHan')");
        });
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.NgayHetHan).HasPrecision(0);
        builder.Property(x => x.BamYeuCau).HasMaxLength(64).IsUnicode(false);
        builder.Property(x => x.LyDoKetThuc).HasMaxLength(500);
        builder.HasIndex(x => new { x.NguoiGuiId, x.MaYeuCau }).IsUnique().HasFilter("[MaYeuCau] IS NOT NULL");
        builder.HasIndex(x => new { x.TrangThai, x.NgayHetHan });
        builder.HasIndex(x => new { x.NguoiNhanId, x.NgayTao });
        builder.HasOne(x => x.TinGui).WithMany().HasForeignKey(x => new { x.TinGuiId, x.SanPhamGuiId }).HasPrincipalKey(x => new { x.Id, x.SanPhamId }).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.TinNhan).WithMany().HasForeignKey(x => new { x.TinNhanId, x.SanPhamNhanId }).HasPrincipalKey(x => new { x.Id, x.SanPhamId }).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.GiaoDichId).IsUnique();
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiGui).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiGuiId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiNhan).WithMany()
            .HasForeignKey(x => new { x.GiaoDichId, x.NguoiNhanId })
            .HasPrincipalKey(x => new { x.GiaoDichId, x.NguoiDungId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SanPhamGui).WithMany()
            .HasForeignKey(x => new { x.SanPhamGuiId, x.NguoiGuiId })
            .HasPrincipalKey(x => new { x.Id, x.ChuSoHuuId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SanPhamNhan).WithMany()
            .HasForeignKey(x => new { x.SanPhamNhanId, x.NguoiNhanId })
            .HasPrincipalKey(x => new { x.Id, x.ChuSoHuuId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}
