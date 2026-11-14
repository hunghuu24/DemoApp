using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReMarket.Infrastructure.Persistence;
using ReMarket.Infrastructure.Seeding;
if(args.Contains("--all"))
{
    var flags=new[]{"Admin__EnableUnlockWorker","Commerce__EnableExpirationWorker","Exchange__EnableExpirationWorker","SmartSwap__EnableExpirationWorker"};
    var previous=flags.ToDictionary(x=>x,Environment.GetEnvironmentVariable);foreach(var flag in flags)Environment.SetEnvironmentVariable(flag,"false");
    using var coverage=new ReMarket.DatabaseChecks.DoPhuApi();var failedSuites=0;
    try
    {
        var suites=new(string Name,Func<Task<int>> Run)[]{("auth",ReMarket.DatabaseChecks.KiemTraTaiKhoan.ChayAsync),("listings",ReMarket.DatabaseChecks.KiemTraTinDang.ChayAsync),("commerce",ReMarket.DatabaseChecks.KiemTraMuaBan.ChayAsync),("exchange",ReMarket.DatabaseChecks.KiemTraTraoDoi.ChayAsync),("smart-swaps",ReMarket.DatabaseChecks.KiemTraSmartSwap.ChayAsync),("smart-swap-workflow",ReMarket.DatabaseChecks.KiemTraNghiepVuSmartSwap.ChayAsync),("smart-delivery",ReMarket.DatabaseChecks.KiemTraGiaoNhanSmartSwap.ChayAsync),("admin",ReMarket.DatabaseChecks.KiemTraQuanTri.ChayAsync)};
        foreach(var suite in suites){Console.WriteLine("BEGIN SUITE "+suite.Name);try{if(await suite.Run()!=0)failedSuites++;}catch(Exception ex){failedSuites++;Console.Error.WriteLine("FAIL SUITE "+suite.Name+": "+ex);}Console.WriteLine("END SUITE "+suite.Name);}
        var index=Array.IndexOf(args,"--coverage-file");await coverage.ExportAsync(index>=0&&index+1<args.Length?args[index+1]:"api-coverage.json");
        Console.WriteLine($"All API suites: {suites.Length-failedSuites}/{suites.Length} passed.");Environment.ExitCode=failedSuites==0?0:1;
    }
    finally{foreach(var flag in flags)Environment.SetEnvironmentVariable(flag,previous[flag]);}
    return;
}
if(args.Contains("--admin")){Environment.ExitCode=await ReMarket.DatabaseChecks.KiemTraQuanTri.ChayAsync();return;}

if(args.Contains("--smart-delivery")){Environment.ExitCode=await ReMarket.DatabaseChecks.KiemTraGiaoNhanSmartSwap.ChayAsync();return;}
if(args.Contains("--smart-swap-workflow")){Environment.ExitCode=await ReMarket.DatabaseChecks.KiemTraNghiepVuSmartSwap.ChayAsync();return;}
if(args.Contains("--smart-swaps")){Environment.ExitCode=await ReMarket.DatabaseChecks.KiemTraSmartSwap.ChayAsync();return;}
if(args.Contains("--exchange")){Environment.ExitCode=await ReMarket.DatabaseChecks.KiemTraTraoDoi.ChayAsync();return;}

if (args.Length == 2 && args[0] == "--export-dictionary")
{
    await ReMarket.DatabaseChecks.XuatTuDien.ChayAsync(args[1]);
    return;
}

if (args.Contains("--commerce"))
{
    Environment.ExitCode = await ReMarket.DatabaseChecks.KiemTraMuaBan.ChayAsync();
    return;
}

if (args.Contains("--listings"))
{
    Environment.ExitCode = await ReMarket.DatabaseChecks.KiemTraTinDang.ChayAsync();
    return;
}

if (args.Contains("--auth"))
{
    Environment.ExitCode = await ReMarket.DatabaseChecks.KiemTraTaiKhoan.ChayAsync();
    return;
}

var factory = new ReMarketDbContextFactory();
await using var db = factory.CreateDbContext([]);
var connectionString = db.Database.GetConnectionString()!;
var targetDatabase = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
if (targetDatabase != "ReMarketDemo" && !targetDatabase.StartsWith("ReMarketTest_", StringComparison.Ordinal))
    throw new InvalidOperationException("Chi chay kiem tra tren ReMarketDemo hoac ReMarketTest_*.");
var passed = 0;
var failed = 0;
void Pass(string name) { passed++; Console.WriteLine("PASS " + name); }
void Fail(string name, Exception? ex = null) { failed++; Console.Error.WriteLine("FAIL " + name + ": " + ex?.Message); }
async Task Reject(string name, FormattableString sql, params int[] expected)
{
    await using var tx = await db.Database.BeginTransactionAsync();
    try { await db.Database.ExecuteSqlInterpolatedAsync(sql); Fail(name + " (SQL accepted invalid data)"); }
    catch (SqlException ex) when (expected.Contains(ex.Number)) { Pass(name); }
    catch (Exception ex) { Fail(name, ex); }
    finally { await tx.RollbackAsync(); }
}
var ma = DuLieuMau.Ma;
if ((await db.Database.GetPendingMigrationsAsync()).Any()) throw new InvalidOperationException("Chua apply migrations.");
if (db.Database.HasPendingModelChanges()) throw new InvalidOperationException("Model va snapshot khong khop.");
Pass("Model matches migration snapshot");

var counts = new Dictionary<string, int>();
await db.Database.OpenConnectionAsync();
async Task<int> CountRows(string table)
{
    using var command = db.Database.GetDbConnection().CreateCommand();
    using var identifierQuoter = new SqlCommandBuilder();
    command.CommandText = "SELECT COUNT(*) FROM [dbo]." + identifierQuoter.QuoteIdentifier(table);
    return Convert.ToInt32(await command.ExecuteScalarAsync());
}
foreach (var entity in db.Model.GetEntityTypes())
{
    var table = entity.GetTableName()!;
    // Table identifiers come exclusively from the compiled EF model, never request data.
    var count = await CountRows(table);
    counts[table] = count;
}
var bootstrapTables = counts.Where(x => x.Key is not ("PhienDangNhap" or "YeuCauDatLaiMatKhau" or "PhieuThuongLuong" or "LichSuHoTro" or "ThongTinGiaoNhanSmartSwap")).ToArray();
if (bootstrapTables.Length == 43 && bootstrapTables.All(x => x.Value > 0)) Pass("All 43 baseline tables have demo data; auth tables may start empty");
else Fail("Demo table coverage");
await DuLieuMau.KhoiTaoAsync(db);
var seedUnchanged = true;
foreach (var (table, before) in counts)
    seedUnchanged &= before == await CountRows(table);
if (seedUnchanged) Pass("Seed is idempotent across all tables"); else Fail("Seed duplicated data");

await Reject("Canonical email unique", $"""
    INSERT dbo.NguoiDung (Id,NgayTao,Email,MatKhauBam,TrangThai)
    VALUES ({Guid.NewGuid()},SYSUTCDATETIME(),N' ANH@REMARKET.EXAMPLE.TEST ',N'demo',N'HoatDong')
    """, 2601, 2627);
await Reject("Negative compensation", $"UPDATE dbo.TienBu SET SoTien=-1 WHERE Id={ma("comp")}", 547);
await Reject("No self exchange", $"UPDATE dbo.DeNghiTraoDoi SET NguoiNhanId=NguoiGuiId WHERE Id={ma("offer")}", 547);
await Reject("Sender must belong to chat", $"""
    INSERT dbo.TinNhan (Id,NgayTao,CuocTroChuyenId,NguoiGuiId,MaPhiaClient,Loai,NoiDung)
    VALUES ({Guid.NewGuid()},SYSUTCDATETIME(),{ma("chat-swap")},{ma("linh")},{Guid.NewGuid()},N'VanBan',N'test')
    """, 547);
await Reject("Rating range", $"UPDATE dbo.DanhGia SET SoSao=6 WHERE Id={ma("review")}", 547);
await Reject("5/6 cannot complete swap", $"UPDATE dbo.VongSmartSwap SET TrangThai=N'HoanTat' WHERE Id={ma("cycle")}", 51006);
await Reject("Cannot revoke finalized consent", $"DELETE dbo.DongYThoaThuan WHERE PhienBanThoaThuanId={ma("swap-agreement")} AND NguoiDungId={ma("vy")}", 51001);
await Reject("Final compensation amount immutable", $"UPDATE dbo.TienBu SET SoTien=600000 WHERE Id={ma("comp-swap-a")}", 51001);
await Reject("Receipt must match swap leg", $"""
    INSERT dbo.XacNhanGiaoNhan (Id,NgayTao,GiaoDichId,NguoiXacNhanId,NguoiDoiTacId,SanPhamId,PhienBanThoaThuanId,Loai,NgayXacNhan)
    VALUES ({Guid.NewGuid()},SYSUTCDATETIME(),{ma("swap")},{ma("anh")},{ma("nam")},{ma("canon")},{ma("swap-agreement")},N'DaNhan',SYSUTCDATETIME())
    """, 51001);
await Reject("No review before completion", $"""
    INSERT dbo.DanhGia (Id,NgayTao,GiaoDichId,NguoiDanhGiaId,NguoiDuocDanhGiaId,SoSao,NoiDung)
    VALUES ({Guid.NewGuid()},SYSUTCDATETIME(),{ma("swap")},{ma("anh")},{ma("nam")},5,N'test')
    """, 51001);
await Reject("One active reservation per item", $"""
    INSERT dbo.GiuChoSanPham (Id,NgayTao,SanPhamId,GiaoDichId,NgayHetHan,DaGiaiPhong)
    VALUES ({Guid.NewGuid()},SYSUTCDATETIME(),{ma("laptop")},{ma("swap")},DATEADD(day,1,SYSUTCDATETIME()),0)
    """, 2601, 2627);
await Reject("Compensation payee must join transaction", $"UPDATE dbo.TienBu SET NguoiNhanId={ma("vy")} WHERE Id={ma("comp")}", 547);
await Reject("Do not close unverified dispute", $"UPDATE dbo.KhieuNai SET TrangThai=N'DaGiaiQuyet' WHERE Id={ma("complaint")}", 51001);
await Reject("Need current 3/3 consent before handover", $"""
    UPDATE dbo.PhienBanThoaThuan SET TrangThai=N'DaThayThe' WHERE Id={ma("swap-agreement")};
    UPDATE dbo.VongSmartSwap SET TrangThai=N'DangBanGiao' WHERE Id={ma("cycle")};
    """, 51003);
await Reject("1-1 requires both receipts", $"UPDATE dbo.GiaoDich SET TrangThai=N'HoanTat',NgayHoanTat=SYSUTCDATETIME() WHERE Id={ma("dispute-trade")}", 51001);

await using (var tx = await db.Database.BeginTransactionAsync())
{
    try
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT dbo.XacNhanGiaoNhan (Id,NgayTao,GiaoDichId,NguoiXacNhanId,NguoiDoiTacId,SanPhamId,PhienBanThoaThuanId,Loai,NgayXacNhan)
            VALUES ({Guid.NewGuid()},SYSUTCDATETIME(),{ma("swap")},{ma("anh")},{ma("nam")},{ma("ipad")},{ma("swap-agreement")},N'DaNhan',SYSUTCDATETIME());
            UPDATE dbo.VongSmartSwap SET TrangThai=N'HoanTat' WHERE Id={ma("cycle")};
            UPDATE dbo.GiaoDich SET TrangThai=N'HoanTat',NgayHoanTat=SYSUTCDATETIME() WHERE Id={ma("swap")};
            INSERT dbo.DanhGia (Id,NgayTao,GiaoDichId,NguoiDanhGiaId,NguoiDuocDanhGiaId,SoSao,NoiDung)
            VALUES ({Guid.NewGuid()},SYSUTCDATETIME(),{ma("swap")},{ma("anh")},{ma("nam")},5,N'Nam đúng hẹn'),
                   ({Guid.NewGuid()},SYSUTCDATETIME(),{ma("swap")},{ma("anh")},{ma("vy")},5,N'Vy giao tiếp tốt');
            """);
        Pass("6/6 completes swap and allows two partner reviews");
    }
    catch (Exception ex) { Fail("Valid swap completion", ex); }
    finally { await tx.RollbackAsync(); }
}

await using (var connection = new SqlConnection(connectionString))
{
    await connection.OpenAsync();
    await using var tx = await connection.BeginTransactionAsync();
    var options = new DbContextOptionsBuilder<ReMarketDbContext>().UseSqlServer(connection).Options;
    await using var first = new ReMarketDbContext(options);
    await using var second = new ReMarketDbContext(options);
    await first.Database.UseTransactionAsync(tx);
    await second.Database.UseTransactionAsync(tx);
    try
    {
        var a = await first.HoSoCaNhan.SingleAsync(x => x.NguoiDungId == ma("anh"));
        var b = await second.HoSoCaNhan.SingleAsync(x => x.NguoiDungId == ma("anh"));
        a.HoTen += " test1"; await first.SaveChangesAsync();
        b.HoTen += " test2";
        try { await second.SaveChangesAsync(); Fail("Stale rowversion was accepted"); }
        catch (DbUpdateConcurrencyException) { Pass("Rowversion rejects stale writes"); }
    }
    catch (Exception ex) { Fail("Rowversion setup", ex); }
    finally { await tx.RollbackAsync(); }
}
Console.WriteLine($"Database checks: {passed} passed, {failed} failed. Negative/positive mutation checks rolled back.");
Environment.ExitCode = failed == 0 ? 0 : 1;
