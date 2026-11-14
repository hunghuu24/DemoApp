using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReMarket.Application.TaiKhoan;
using ReMarket.Domain.Enums;
using ReMarket.Domain.Models;
using ReMarket.Infrastructure.Persistence;

namespace ReMarket.Infrastructure.Authentication;

public sealed class TaiKhoanService(ReMarketDbContext db, IPasswordHasher<NguoiDung> hasher,
    TokenService tokens, IOptions<TuyChonJwt> settings, IGuiThuKhoiPhuc mail) : ITaiKhoanService
{
    private static string Email(string value) => value.Trim().ToUpperInvariant();
    private static string DienThoai(string value)
    {
        var phone = value.Trim().Replace(" ", "").Replace("-", "");
        if (phone.StartsWith("+84", StringComparison.Ordinal)) phone = "0" + phone[3..];
        if (phone.Length != 10 || phone[0] != '0' || !phone.All(char.IsAsciiDigit))
            throw new LoiTaiKhoan(400, "Số điện thoại Việt Nam phải có 10 chữ số, bắt đầu bằng 0.");
        return phone;
    }
    private static void KiemTraMatKhau(string password, string confirm)
    {
        if (password.Length is < 8 or > 128 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new LoiTaiKhoan(400, "Mật khẩu phải từ 8 đến 128 ký tự, gồm chữ và số.");
        if (password != confirm) throw new LoiTaiKhoan(400, "Xác nhận mật khẩu chưa khớp.");
    }
    private static bool TrungDuLieu(DbUpdateException ex) => ex.InnerException is SqlException { Number: 2601 or 2627 };
    private async Task<string[]> VaiTroAsync(Guid id, CancellationToken ct) => await db.NguoiDungVaiTro
        .Where(x => x.NguoiDungId == id).Select(x => x.VaiTro.Ma).OrderBy(x => x).ToArrayAsync(ct);
    public async Task<HoSoDto> LayHoSoAsync(Guid id, CancellationToken ct)
    {
        var profile = await db.HoSoCaNhan.AsNoTracking().Include(x => x.NguoiDung).SingleOrDefaultAsync(x => x.NguoiDungId == id, ct)
            ?? throw new LoiTaiKhoan(404, "Không tìm thấy hồ sơ.");
        return new(id, profile.HoTen, profile.NguoiDung.Email, profile.NguoiDung.SoDienThoai, profile.DiaChi,
            profile.TinhThanh, profile.AnhDaiDienUrl is null ? null : "api/taikhoan/anh-dai-dien?v=" + Uri.EscapeDataString(Convert.ToBase64String(profile.PhienBan)),
            profile.DaXacMinhEmail, profile.DaXacMinhDienThoai, await VaiTroAsync(id, ct),
            Convert.ToBase64String(profile.PhienBan), profile.NguoiDung.NgayTao);
    }
    public async Task<HoSoDto> DangKyAsync(DangKyRequest request, CancellationToken ct)
    {
        KiemTraMatKhau(request.MatKhau, request.XacNhanMatKhau);
        if (!request.DongYDieuKhoan) throw new LoiTaiKhoan(400, "Bạn cần đồng ý điều khoản để đăng ký.");
        var email = request.Email.Trim();
        var normalized = Email(email);
        var phone = DienThoai(request.SoDienThoai);
        if (string.IsNullOrWhiteSpace(request.HoTen) || request.HoTen.Trim().Length < 2)
            throw new LoiTaiKhoan(400, "Vui lòng nhập họ tên.");
        if (await db.NguoiDung.AnyAsync(x => x.EmailChuanHoa == normalized || x.SoDienThoai == phone, ct))
            throw new LoiTaiKhoan(409, "Email hoặc số điện thoại đã được sử dụng.");
        var role = await db.VaiTro.SingleAsync(x => x.Ma == "USER", ct);
        var user = new NguoiDung { Email = email, SoDienThoai = phone, TrangThai = TrangThaiNguoiDung.HoatDong };
        user.MatKhauBam = hasher.HashPassword(user, request.MatKhau);
        db.Add(user);
        db.Add(new HoSoCaNhan { NguoiDungId = user.Id, HoTen = request.HoTen.Trim() });
        db.Add(new NguoiDungVaiTro { NguoiDungId = user.Id, VaiTroId = role.Id });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (TrungDuLieu(ex)) { throw new LoiTaiKhoan(409, "Email hoặc số điện thoại đã được sử dụng."); }
        return await LayHoSoAsync(user.Id, ct);
    }
    public async Task<DangNhapDto> DangNhapAsync(DangNhapRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var account = request.TaiKhoan.Trim();
        string? phone = null;
        if (!account.Contains('@')) { try { phone = DienThoai(account); } catch (LoiTaiKhoan) { /* Use generic login failure. */ } }
        var email = Email(account);
        var user = await db.NguoiDung.FromSqlInterpolated($"SELECT * FROM dbo.NguoiDung WITH (UPDLOCK, ROWLOCK) WHERE EmailChuanHoa={email} OR SoDienThoai={phone}")
            .SingleOrDefaultAsync(ct);
        // A real verification also occurs for unknown accounts, avoiding a fast unknown-user path.
        var candidate = user ?? new NguoiDung();
        var dummyHash = hasher.HashPassword(candidate, TokenService.TaoBiMat());
        var result = hasher.VerifyHashedPassword(candidate, user?.MatKhauBam ?? dummyHash, request.MatKhau);
        if (user is null || result == PasswordVerificationResult.Failed || user.TrangThai != TrangThaiNguoiDung.HoatDong)
            throw new LoiTaiKhoan(401, "Thông tin đăng nhập không đúng hoặc tài khoản không khả dụng.");
        if (result == PasswordVerificationResult.SuccessRehashNeeded) user.MatKhauBam = hasher.HashPassword(user, request.MatKhau);
        var refresh = TokenService.TaoBiMat();
        var session = new PhienDangNhap { NguoiDungId = user.Id, MaTruyCap = Guid.NewGuid().ToString("N"),
            BamRefreshToken = TokenService.Bam(refresh), NgayHetHan = DateTimeOffset.UtcNow.AddDays(settings.Value.RefreshDays) };
        db.Add(session);
        await db.SaveChangesAsync(ct);
        var response = await PhanHoiPhienAsync(session, refresh, ct);
        await tx.CommitAsync(ct);
        return response;
    }
    private async Task<DangNhapDto> PhanHoiPhienAsync(PhienDangNhap session, string refresh, CancellationToken ct)
    {
        var profile = await LayHoSoAsync(session.NguoiDungId, ct);
        var access = tokens.TaoAccess(session, profile.VaiTro);
        return new(access.Token, refresh, access.HetHan, session.NgayHetHan, profile);
    }
    public async Task<DangNhapDto> LamMoiAsync(string refreshToken, CancellationToken ct)
    {
        var hash = TokenService.Bam(refreshToken);
        var now = DateTimeOffset.UtcNow;
        var session = await db.PhienDangNhap.Include(x => x.NguoiDung).SingleOrDefaultAsync(x => x.BamRefreshToken == hash, ct);
        if (session is null || session.NgayThuHoi != null || session.NgayHetHan <= now || session.NguoiDung.TrangThai != TrangThaiNguoiDung.HoatDong)
            throw new LoiTaiKhoan(401, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        var next = TokenService.TaoBiMat();
        session.BamRefreshToken = TokenService.Bam(next);
        session.MaTruyCap = Guid.NewGuid().ToString("N");
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new LoiTaiKhoan(401, "Phiên đã được làm mới hoặc thu hồi."); }
        return await PhanHoiPhienAsync(session, next, ct);
    }
    public async Task DangXuatAsync(Guid id, Guid sessionId, CancellationToken ct)
    {
        await db.PhienDangNhap.Where(x => x.Id == sessionId && x.NguoiDungId == id && x.NgayThuHoi == null)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.NgayThuHoi, DateTimeOffset.UtcNow), ct);
    }
    public async Task QuenMatKhauAsync(string email, CancellationToken ct)
    {
        var normalized = Email(email);
        var user = await db.NguoiDung.SingleOrDefaultAsync(x => x.EmailChuanHoa == normalized && x.TrangThai == TrangThaiNguoiDung.HoatDong, ct);
        if (user is null) return;
        var secret = TokenService.TaoBiMat();
        var reset = new YeuCauDatLaiMatKhau { NguoiDungId = user.Id, BamToken = TokenService.Bam(secret), NgayHetHan = DateTimeOffset.UtcNow.AddMinutes(15) };
        db.Add(reset);
        await db.SaveChangesAsync(ct);
        await mail.GuiAsync(user.Email, secret, ct);
    }
    public async Task DatLaiMatKhauAsync(DatLaiMatKhauRequest request, CancellationToken ct)
    {
        KiemTraMatKhau(request.MatKhauMoi, request.XacNhanMatKhau);
        var hash = TokenService.Bam(request.Token);
        var normalized = Email(request.Email);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var user = await db.NguoiDung.FromSqlInterpolated($"SELECT * FROM dbo.NguoiDung WITH (UPDLOCK, ROWLOCK) WHERE EmailChuanHoa={normalized}").SingleOrDefaultAsync(ct);
        var reset = user is null ? null : await db.YeuCauDatLaiMatKhau.Include(x => x.NguoiDung)
            .SingleOrDefaultAsync(x => x.BamToken == hash && x.NguoiDungId == user.Id, ct);
        if (reset is null || reset.NgaySuDung != null || reset.NgayHetHan <= DateTimeOffset.UtcNow || reset.NguoiDung.TrangThai != TrangThaiNguoiDung.HoatDong)
            throw new LoiTaiKhoan(400, "Liên kết khôi phục không hợp lệ hoặc đã hết hạn.");
        reset.NguoiDung.MatKhauBam = hasher.HashPassword(reset.NguoiDung, request.MatKhauMoi);
        reset.NgaySuDung = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await ThuHoiTatCaAsync(reset.NguoiDungId, ct);
        await db.YeuCauDatLaiMatKhau.Where(x => x.NguoiDungId == reset.NguoiDungId && x.NgaySuDung == null)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.NgaySuDung, DateTimeOffset.UtcNow), ct);
        await tx.CommitAsync(ct);
    }
    public async Task DoiMatKhauAsync(Guid id, DoiMatKhauRequest request, CancellationToken ct)
    {
        KiemTraMatKhau(request.MatKhauMoi, request.XacNhanMatKhau);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var user = await db.NguoiDung.FromSqlInterpolated($"SELECT * FROM dbo.NguoiDung WITH (UPDLOCK, ROWLOCK) WHERE Id={id}").SingleAsync(ct);
        if (hasher.VerifyHashedPassword(user, user.MatKhauBam, request.MatKhauCu) == PasswordVerificationResult.Failed)
            throw new LoiTaiKhoan(400, "Mật khẩu hiện tại chưa đúng.");
        user.MatKhauBam = hasher.HashPassword(user, request.MatKhauMoi);
        await db.SaveChangesAsync(ct);
        await ThuHoiTatCaAsync(id, ct);
        await tx.CommitAsync(ct);
    }
    private Task<int> ThuHoiTatCaAsync(Guid id, CancellationToken ct) => db.PhienDangNhap.Where(x => x.NguoiDungId == id && x.NgayThuHoi == null)
        .ExecuteUpdateAsync(x => x.SetProperty(p => p.NgayThuHoi, DateTimeOffset.UtcNow), ct);
    public async Task<HoSoDto> CapNhatHoSoAsync(Guid id, CapNhatHoSoRequest request, CancellationToken ct)
    {
        var phone = DienThoai(request.SoDienThoai);
        if (string.IsNullOrWhiteSpace(request.HoTen) || request.HoTen.Trim().Length < 2) throw new LoiTaiKhoan(400, "Vui lòng nhập họ tên.");
        byte[] expected;
        try { expected = Convert.FromBase64String(request.PhienBan); }
        catch (FormatException) { throw new LoiTaiKhoan(400, "Phiên bản hồ sơ không hợp lệ."); }
        if (expected.Length != 8) throw new LoiTaiKhoan(400, "Phiên bản hồ sơ không hợp lệ.");
        var normalized = Email(request.Email);
        if (await db.NguoiDung.AnyAsync(x => x.Id != id && (x.EmailChuanHoa == normalized || x.SoDienThoai == phone), ct))
            throw new LoiTaiKhoan(409, "Email hoặc số điện thoại đã được sử dụng.");
        var profile = await db.HoSoCaNhan.Include(x => x.NguoiDung).SingleAsync(x => x.NguoiDungId == id, ct);
        db.Entry(profile).Property(x => x.PhienBan).OriginalValue = expected;
        if (!string.Equals(profile.NguoiDung.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase)) profile.DaXacMinhEmail = false;
        if (profile.NguoiDung.SoDienThoai != phone) profile.DaXacMinhDienThoai = false;
        profile.NguoiDung.Email = request.Email.Trim(); profile.NguoiDung.SoDienThoai = phone;
        profile.HoTen = request.HoTen.Trim(); profile.DiaChi = request.DiaChi?.Trim(); profile.TinhThanh = request.TinhThanh?.Trim();
        // Force a rowversion predicate even when only account fields changed.
        db.Entry(profile).Property(x => x.HoTen).IsModified = true;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new LoiTaiKhoan(409, "Hồ sơ vừa thay đổi. Hãy tải lại trước khi lưu."); }
        catch (DbUpdateException ex) when (TrungDuLieu(ex)) { throw new LoiTaiKhoan(409, "Email hoặc số điện thoại đã được sử dụng."); }
        return await LayHoSoAsync(id, ct);
    }
}
