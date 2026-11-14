using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReMarket.Application.TaiKhoan;
using ReMarket.Infrastructure.Persistence;
using SkiaSharp;

namespace ReMarket.Infrastructure.Authentication;

public sealed class AnhDaiDienService(ReMarketDbContext db, IConfiguration config, IHostEnvironment env, ITaiKhoanService accounts)
{
    private string Folder => config["Media:AvatarFolder"] ?? Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", ".local", "avatars"));
    public async Task<HoSoDto> LuuAsync(Guid id, Stream stream, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, ct);
        if (buffer.Length is 0 or > 2 * 1024 * 1024) throw new LoiTaiKhoan(400, "Ảnh phải là JPG/PNG, tối đa 2 MB.");
        buffer.Position = 0;
        string file;
        using var input = new SKManagedStream(buffer);
        using var codec = SKCodec.Create(input) ?? throw new LoiTaiKhoan(400, "Không đọc được ảnh JPG/PNG.");
        if (codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png))
            throw new LoiTaiKhoan(400, "Chỉ hỗ trợ ảnh JPG hoặc PNG.");
        var info = codec.Info;
        if (info.Width > 4096 || info.Height > 4096 || (long)info.Width * info.Height > 8_000_000)
            throw new LoiTaiKhoan(400, "Kích thước ảnh quá lớn.");
        using var bitmap = new SKBitmap(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            throw new LoiTaiKhoan(400, "Ảnh bị hỏng hoặc chưa đầy đủ.");
        using var image = SKImage.FromBitmap(bitmap);
        using var surface = SKSurface.Create(new SKImageInfo(512, 512));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.Translate(256, 256);
        canvas.Scale(512f / Math.Min(info.Width, info.Height));
        switch (codec.EncodedOrigin)
        {
            case SKEncodedOrigin.TopRight: canvas.Scale(-1, 1); break;
            case SKEncodedOrigin.BottomRight: canvas.RotateDegrees(180); break;
            case SKEncodedOrigin.BottomLeft: canvas.Scale(1, -1); break;
            case SKEncodedOrigin.LeftTop: canvas.RotateDegrees(90); canvas.Scale(1, -1); break;
            case SKEncodedOrigin.RightTop: canvas.RotateDegrees(90); break;
            case SKEncodedOrigin.RightBottom: canvas.RotateDegrees(90); canvas.Scale(-1, 1); break;
            case SKEncodedOrigin.LeftBottom: canvas.RotateDegrees(270); break;
        }
        canvas.DrawImage(image, -info.Width / 2f, -info.Height / 2f, new SKSamplingOptions(SKFilterMode.Linear));
        using var output = surface.Snapshot();
        using var encoded = output.Encode(SKEncodedImageFormat.Jpeg, 85);
        Directory.CreateDirectory(Folder);
        file = Guid.NewGuid().ToString("N") + ".jpg";
        await File.WriteAllBytesAsync(Path.Combine(Folder, file), encoded.ToArray(), ct);
        var profile = await db.HoSoCaNhan.SingleAsync(x => x.NguoiDungId == id, ct);
        profile.AnhDaiDienUrl = file;
        try { await db.SaveChangesAsync(ct); }
        catch { File.Delete(Path.Combine(Folder, file)); throw; }
        return await accounts.LayHoSoAsync(id, ct);
    }
    public async Task<string> DuongDanAsync(Guid id, CancellationToken ct)
    {
        var name = await db.HoSoCaNhan.Where(x => x.NguoiDungId == id).Select(x => x.AnhDaiDienUrl).SingleAsync(ct);
        if (name is null || name != Path.GetFileName(name) || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _) || Path.GetExtension(name) != ".jpg")
            throw new LoiTaiKhoan(404, "Chưa có ảnh đại diện.");
        var file = Path.Combine(Folder, name);
        if (!File.Exists(file)) throw new LoiTaiKhoan(404, "Không tìm thấy ảnh đại diện.");
        return file;
    }
}
