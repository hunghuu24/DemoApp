using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

public sealed class NhuCauTraoDoiConfiguration : IEntityTypeConfiguration<NhuCauTraoDoi>
{
    public void Configure(EntityTypeBuilder<NhuCauTraoDoi> builder)
    {
        CauHinhCoBan.ApDung(builder);
        builder.ToTable("NhuCauTraoDoi", "dbo", table =>
        {
            table.UseSqlOutputClause(false);
            table.HasCheckConstraint("CK_NhuCauTraoDoi_TienBu", "[SoTienBuToiDa] >= 0 AND ([ChoPhepBuTien] = 1 OR [SoTienBuToiDa] = 0)");
            table.HasCheckConstraint("CK_NhuCauTraoDoi_HinhThuc", "[ChoPhepMotMot]=1 OR [ChoPhepSmartSwap]=1");
            table.HasCheckConstraint("CK_NhuCauTraoDoi_GiaMongMuon", "([GiaTriMongMuonTu] IS NULL OR [GiaTriMongMuonTu] BETWEEN 0 AND 1000000000000) AND ([GiaTriMongMuonDen] IS NULL OR [GiaTriMongMuonDen] BETWEEN 0 AND 1000000000000) AND ([GiaTriMongMuonTu] IS NULL OR [GiaTriMongMuonDen] IS NULL OR [GiaTriMongMuonTu]<=[GiaTriMongMuonDen])");
            table.HasCheckConstraint("CK_NhuCauTraoDoi_TinhTrang", "[TinhTrangToiThieu] IS NULL OR [TinhTrangToiThieu] IN ('NhuMoi','DaSuDungTot','CoLoiNhe')");
        });
        builder.Property(x => x.MoTaMongMuon).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ChoPhepMotMot).HasDefaultValue(true);
        builder.Property(x => x.GhiChu).HasMaxLength(1000);
        builder.Property(x => x.TuKhoaMongMuon).HasMaxLength(200);
        builder.Property(x => x.TinhTrangToiThieu).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(x => x.TinDangId).IsUnique();
        builder.HasOne(x => x.TinDang).WithMany()
            .HasForeignKey(x => x.TinDangId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.DanhMucMongMuon).WithMany()
            .HasForeignKey(x => x.DanhMucMongMuonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
