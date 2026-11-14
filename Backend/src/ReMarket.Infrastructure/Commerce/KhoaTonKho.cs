using Microsoft.EntityFrameworkCore;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.Commerce;

/// <summary>All catalog/order stock mutations lock product, then listing, then order/chat.</summary>
public sealed class KhoaTonKho(ReMarketDbContext db)
{
    public async Task<Dictionary<Guid,TinDang>> NhieuTinAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var keys=ids.Distinct().Order().ToArray();
        var products=await db.TinDang.AsNoTracking().Where(x=>keys.Contains(x.Id)).Select(x=>new{x.Id,x.SanPhamId}).ToListAsync(ct);
        if(products.Count!=keys.Length)throw new LoiTaiKhoan(404,"Không tìm thấy tin trao đổi.");
        // Acquire every product before every listing; all two-item operations use the same order.
        foreach(var productId in products.Select(x=>x.SanPhamId).Distinct().Order())
        {
            var product=await db.SanPham.FromSqlInterpolated($"SELECT * FROM dbo.SanPham WITH (UPDLOCK, ROWLOCK) WHERE Id={productId}").SingleAsync(ct);
            await db.Entry(product).ReloadAsync(ct);
        }
        var result=new Dictionary<Guid,TinDang>();
        foreach(var key in keys)
        {
            var listing=await db.TinDang.FromSqlInterpolated($"SELECT * FROM dbo.TinDang WITH (UPDLOCK, ROWLOCK) WHERE Id={key}").SingleAsync(ct);
            await db.Entry(listing).ReloadAsync(ct);result.Add(key,listing);
        }
        return result;
    }
    public async Task<(TinDang Tin, ReMarket.Domain.Models.SanPham SanPham)> TinAsync(Guid id, CancellationToken ct)
    {
        var productId = await db.TinDang.AsNoTracking().Where(t => t.Id == id).Select(t => (Guid?)t.SanPhamId).SingleOrDefaultAsync(ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy tin.");
        var product = await db.SanPham.FromSqlInterpolated($"SELECT * FROM dbo.SanPham WITH (UPDLOCK, ROWLOCK) WHERE Id={productId}").SingleAsync(ct);
        var tin = await db.TinDang.FromSqlInterpolated($"SELECT * FROM dbo.TinDang WITH (UPDLOCK, ROWLOCK) WHERE Id={id}").SingleAsync(ct);
        return (tin, product);
    }
    public static void PhienBan(byte[] actual, string expected)
    {
        byte[] value;
        try { value = Convert.FromBase64String(expected); } catch (FormatException) { throw new LoiTaiKhoan(400, "Phiên bản không hợp lệ."); }
        if (!actual.SequenceEqual(value)) throw new LoiTaiKhoan(409, "Dữ liệu vừa thay đổi. Hãy tải lại trước khi thao tác.");
    }
}
