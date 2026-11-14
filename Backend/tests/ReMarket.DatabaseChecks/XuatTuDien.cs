using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.DatabaseChecks;

internal static class XuatTuDien
{
    public static async Task ChayAsync(string path)
    {
        var old = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        var groups = old.RootElement.GetProperty("entities").EnumerateArray()
            .ToDictionary(x => x.GetProperty("name").GetString()!, x => x.GetProperty("group").GetString()!);
        groups["PhieuThuongLuong"] = "DonHang"; groups["LichSuHoTro"] = "HoTro";
        await using var db = new ReMarketDbContextFactory().CreateDbContext([]);
        var model = db.GetService<IDesignTimeModel>().Model;
        string TypeName(IProperty p)
        {
            var type = Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType;
            var name = type == typeof(string) ? "string" : type == typeof(long) ? "long" : type == typeof(int) ? "int"
                : type == typeof(bool) ? "bool" : type.Name;
            return name + (p.IsNullable ? "?" : "") + (p.GetMaxLength() is int len ? ":" + len : "");
        }
        var entities = model.GetEntityTypes().OrderBy(x => x.GetTableName()).Select(e => new {
            group = groups.GetValueOrDefault(e.GetTableName()!, "Khac"), name = e.GetTableName(),
            fields = e.GetProperties().Where(p => p.Name is not ("Id" or "NgayTao" or "PhienBan")).ToDictionary(p => p.Name, TypeName),
            unique = e.GetIndexes().Where(i => i.IsUnique).Select(i => i.Properties.Select(p => p.Name).ToArray()),
            uniqueFilters = e.GetIndexes().Where(i => i.IsUnique && i.GetFilter() != null).Select(i => new {
                fields = i.Properties.Select(p => p.Name).ToArray(), sql = i.GetFilter() }),
            fks = e.GetForeignKeys().Select(f => new { nav = f.DependentToPrincipal?.Name, target = f.PrincipalEntityType.GetTableName(),
                fields = f.Properties.Select(p => p.Name).ToArray(), principal = f.PrincipalKey.Properties.Select(p => p.Name).ToArray() }),
            computed = e.GetProperties().Where(p => p.GetComputedColumnSql() != null).ToDictionary(p => p.Name, p => p.GetComputedColumnSql()),
            checks = e.GetCheckConstraints().Select(c => new { name = c.Name, sql = c.Sql })
        }).ToArray();
        var enums = typeof(IThucThe).Assembly.GetTypes().Where(t => t.IsEnum).OrderBy(t => t.Name).ToDictionary(t => t.Name, Enum.GetNames);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new { enums, entities }, new JsonSerializerOptions {
            WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }) + Environment.NewLine);
        Console.WriteLine($"Exported {entities.Length} entity definitions from the EF design-time model.");
    }
}
