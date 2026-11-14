using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class CuocTroChuyenConfiguration : IEntityTypeConfiguration<CuocTroChuyen>
{
    public void Configure(EntityTypeBuilder<CuocTroChuyen> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("CuocTroChuyen", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_CuocTroChuyen_Loai", "[Loai] IN (N'TrucTiep', N'NhomSmartSwap')");
        });
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(40).IsUnicode(false);
        builder.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
        builder.HasOne<NguoiDung>().WithMany().HasForeignKey(x => x.NguoiMuaId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => new { x.TinDangId, x.NguoiMuaId }).IsUnique().HasFilter("[NguoiMuaId] IS NOT NULL");
        builder.HasIndex(x => x.GiaoDichId).IsUnique();
        builder.HasOne(x => x.GiaoDich).WithMany()
            .HasForeignKey(x => x.GiaoDichId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.TinDang).WithMany()
            .HasForeignKey(x => x.TinDangId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
