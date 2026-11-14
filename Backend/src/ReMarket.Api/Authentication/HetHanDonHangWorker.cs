using ReMarket.Infrastructure.Commerce;

namespace ReMarket.Api.Authentication;

public sealed class HetHanDonHangWorker(IServiceScopeFactory scopes, ILogger<HetHanDonHangWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<MuaBanService>().HetHanAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogWarning(ex, "Chua giai phong duoc don het han; se thu lai o lan sau."); }
        }
    }
}
