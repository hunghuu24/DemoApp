using Microsoft.AspNetCore.Diagnostics;
using ReMarket.Application.TaiKhoan;

namespace ReMarket.Api.Authentication;

public sealed class XuLyLoiTaiKhoan : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not LoiTaiKhoan error) return false;
        context.Response.StatusCode = error.Status;
        await context.Response.WriteAsJsonAsync(new { title = "Không thực hiện được yêu cầu", detail = error.Message, status = error.Status }, ct);
        return true;
    }
}
