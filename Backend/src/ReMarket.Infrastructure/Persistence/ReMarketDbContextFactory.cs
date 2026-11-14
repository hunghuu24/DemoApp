using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReMarket.Infrastructure.Persistence;

public sealed class ReMarketDbContextFactory : IDesignTimeDbContextFactory<ReMarketDbContext>
{
    public ReMarketDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ReMarket")
            ?? "Server=localhost;Database=ReMarketDemo;Trusted_Connection=True;TrustServerCertificate=True";
        return new ReMarketDbContext(new DbContextOptionsBuilder<ReMarketDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ReMarketDbContext).Assembly.FullName))
            .Options);
    }
}
