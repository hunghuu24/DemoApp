using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReMarket.Domain.Models;

namespace ReMarket.Infrastructure.Configurations;

internal static class CauHinhCoBan
{
    public static void ApDung<T>(EntityTypeBuilder<T> builder) where T : class, IThucThe
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.NgayTao).HasPrecision(0).IsRequired();
        builder.Property(x => x.PhienBan).IsRowVersion();
    }
}
