using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReMarket.Application.TaiKhoan;
using SkiaSharp;

namespace ReMarket.Infrastructure.Catalog;

/// <summary>Private files; HTTP access is checked against the listing, not the original filename.</summary>
public sealed class KhoAnhSanPham(IConfiguration config, IHostEnvironment env)
{
    private static readonly string[] DemoFiles = ["/demo/san-pham.png", "/demo/figma-ban.jpg", "/demo/figma-ke.jpg", "/demo/figma-iphone.png"];
    private string Folder => config["Media:ProductFolder"] ?? Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", ".local", "products"));
    public async Task<string> LuuAsync(Stream input, CancellationToken ct)
    {
        using var buffer = new MemoryStream();
        var bytes = new byte[16 * 1024];
        int read;
        while ((read = await input.ReadAsync(bytes, ct)) > 0)
        {
            buffer.Write(bytes, 0, read);
            if (buffer.Length > 5 * 1024 * 1024) throw new LoiTaiKhoan(400, "Mỗi ảnh tối đa 5 MB.");
        }
        buffer.Position = 0;
        using var managed = new SKManagedStream(buffer);
        using var codec = SKCodec.Create(managed) ?? throw new LoiTaiKhoan(400, "Không đọc được ảnh JPG/PNG.");
        if (codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png))
            throw new LoiTaiKhoan(400, "Chỉ hỗ trợ ảnh JPG hoặc PNG.");
        var info = codec.Info;
        if (info.Width <= 0 || info.Height <= 0 || info.Width > 6000 || info.Height > 6000 || (long)info.Width * info.Height > 16_000_000)
            throw new LoiTaiKhoan(400, "Ảnh tối đa 16 megapixel, cạnh không quá 6000 pixel.");
        using var bitmap = new SKBitmap(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            throw new LoiTaiKhoan(400, "Ảnh bị hỏng hoặc chưa đầy đủ.");
        var rotate = codec.EncodedOrigin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var width = rotate ? info.Height : info.Width;
        var height = rotate ? info.Width : info.Height;
        var scale = Math.Min(1f, 1600f / Math.Max(width, height));
        using var surface = SKSurface.Create(new SKImageInfo(Math.Max(1, (int)(width * scale)), Math.Max(1, (int)(height * scale))));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.Translate(surface.Canvas.LocalClipBounds.Width / 2, surface.Canvas.LocalClipBounds.Height / 2);
        canvas.Scale(scale);
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
        using var image = SKImage.FromBitmap(bitmap);
        canvas.DrawImage(image, -info.Width / 2f, -info.Height / 2f, new SKSamplingOptions(SKFilterMode.Linear));
        using var snapshot = surface.Snapshot();
        using var encoded = snapshot.Encode(SKEncodedImageFormat.Jpeg, 85);
        Directory.CreateDirectory(Folder);
        var file = Guid.NewGuid().ToString("N") + ".jpg";
        await File.WriteAllBytesAsync(Path.Combine(Folder, file), encoded.ToArray(), ct);
        return file;
    }
    public string DuongDan(string file)
    {
        // Preserve the existing demo fixture; arbitrary paths/remote URLs are never accepted.
        if (DemoFiles.Contains(file)) return Path.Combine(env.ContentRootPath, "wwwroot", "demo", Path.GetFileName(file));
        if (file != Path.GetFileName(file) || Path.GetExtension(file) != ".jpg" ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out _))
            throw new LoiTaiKhoan(404, "Không tìm thấy ảnh.");
        return Path.Combine(Folder, file);
    }
    public void Xoa(string file)
    {
        if (DemoFiles.Contains(file)) return;
        try { File.Delete(DuongDan(file)); } catch (IOException) { /* Committed DB state takes precedence; an orphan can be cleaned later. */ }
    }
}
