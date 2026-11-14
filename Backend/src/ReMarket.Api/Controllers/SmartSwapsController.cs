using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReMarket.Application.SmartSwap;
using ReMarket.Infrastructure.SmartSwap;

namespace ReMarket.Api.Controllers;

[ApiController, Authorize(Roles="USER"), Route("api/smart-swaps")]
public sealed class SmartSwapsController(TimSmartSwapService service) : ControllerBase
{
    [HttpGet("search"), EnableRateLimiting("smart-swap-search")]
    public Task<KetQuaTimSmartSwapDto> Search([FromQuery] TimSmartSwapRequest request, CancellationToken ct)
    {
        Response.Headers.CacheControl = "private, no-store";
        return service.TimAsync(request,ct);
    }
}
