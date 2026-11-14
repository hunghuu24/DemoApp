using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class TinDangConfiguration : IEntityTypeConfiguration<TinDang>
{
    public void Configure(EntityTypeBuilder<TinDang> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("TinDang", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_TinDang_GiaBan", "([LoaiTin] = N'TraoDoi' AND ([GiaBan] IS NULL OR [GiaBan] >= 0)) OR ([LoaiTin] IN (N'MuaBan', N'CaHai') AND [GiaBan] IS NOT NULL AND [GiaBan] > 0)");
            table.HasCheckConstraint("CK_TinDang_LoaiTin", "[LoaiTin] IN (N'MuaBan', N'TraoDoi', N'CaHai')");
            table.HasCheckConstraint("CK_TinDang_TrangThai", "[TrangThai] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan', N'DangGiuCho', N'DaTraoDoi')");
            table.HasCheckConstraint("CK_TinDang_LuotXem", "[LuotXem] >= 0");
        });
        builder.Property(x => x.LoaiTin).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.TinhThanh).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DiaDiem).HasMaxLength(500).IsRequired();
        builder.Property(x => x.NgayDang).HasPrecision(0);
        builder.Property(x => x.NgayCapNhat).HasPrecision(0);
        builder.Property(x => x.NgayXoa).HasPrecision(0);
        builder.HasIndex(x => new { x.NgayXoa, x.TrangThai, x.LoaiTin, x.NgayDang });
        builder.HasIndex(x => new { x.NguoiDangId, x.NgayXoa, x.TrangThai });
        builder.Property(x => x.LyDoTuChoi).HasMaxLength(1000);
        builder.HasAlternateKey(x => new { x.Id, x.SanPhamId });
        builder.HasIndex(x => x.SanPhamId).IsUnique().HasFilter("[TrangThai] IN ('ChoDuyet', 'DangHienThi')");
        builder.HasOne(x => x.SanPham).WithMany()
            .HasForeignKey(x => new { x.SanPhamId, x.NguoiDangId })
            .HasPrincipalKey(x => new { x.Id, x.ChuSoHuuId })
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.NguoiDuyet).WithMany()
            .HasForeignKey(x => x.NguoiDuyetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
