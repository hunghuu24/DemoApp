using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ReMarket.Application.TaiKhoan;

namespace ReMarket.Infrastructure.Authentication;

public sealed class GuiThuKhoiPhuc(IConfiguration config, IHostEnvironment environment) : IGuiThuKhoiPhuc
{
    public async Task GuiAsync(string email, string token, CancellationToken ct)
    {
        var baseUrl = config["Recovery:BaseUrl"] ?? (environment.IsDevelopment() ? "remarket://tai-khoan/dat-lai-mat-khau" : "");
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var recoveryUri) || !string.IsNullOrEmpty(recoveryUri.Query) ||
            !string.IsNullOrEmpty(recoveryUri.Fragment) || (!environment.IsDevelopment() && recoveryUri.Scheme != "https"))
            throw new LoiTaiKhoan(503, "Dịch vụ khôi phục chưa sẵn sàng.");
        var link = baseUrl + "?email=" + Uri.EscapeDataString(email) + "&token=" + Uri.EscapeDataString(token);
        var host = config["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            if (!environment.IsDevelopment()) throw new LoiTaiKhoan(503, "Dịch vụ gửi thư chưa sẵn sàng.");
            var folder = config["Smtp:DevelopmentOutbox"] ?? Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", ".local", "mail"));
            Directory.CreateDirectory(folder);
            var contents = JsonSerializer.Serialize(new { Den = email, LienKet = link, NgayGuiUtc = DateTimeOffset.UtcNow });
            await File.WriteAllTextAsync(Path.Combine(folder, Guid.NewGuid().ToString("N") + ".json"), contents, ct);
            return;
        }
        try
        {
            using var client = new SmtpClient(host, config.GetValue("Smtp:Port", 587)) {
                EnableSsl = config.GetValue("Smtp:EnableSsl", true),
                Credentials = new NetworkCredential(config["Smtp:Username"], config["Smtp:Password"])
            };
            using var message = new MailMessage(config["Smtp:From"] ?? throw new InvalidOperationException("Smtp:From is required"), email) {
                Subject = "ReMarket — Khôi phục mật khẩu", IsBodyHtml = false,
                Body = "Bạn đã yêu cầu đặt lại mật khẩu ReMarket. Liên kết có hiệu lực trong 15 phút và chỉ dùng một lần:\n" + link +
                    "\nNếu bạn không yêu cầu, hãy bỏ qua thư này. Không chia sẻ liên kết với người khác."
            };
            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or FormatException)
        { throw new LoiTaiKhoan(503, "Chưa gửi được thư khôi phục. Vui lòng thử lại sau."); }
    }
}
