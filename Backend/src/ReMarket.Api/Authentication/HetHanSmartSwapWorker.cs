using ReMarket.Infrastructure.SmartSwap;

namespace ReMarket.Api.Authentication;

public sealed class HetHanSmartSwapWorker(IServiceScopeFactory scopes,ILogger<HetHanSmartSwapWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30),token);
                await using var scope=scopes.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<VongSmartSwapService>().HetHanAsync(token);
            }
            catch(OperationCanceledException)when(token.IsCancellationRequested){break;}
            catch(Exception ex){logger.LogWarning(ex,"Chua xu ly duoc loi moi Smart Swap het han; se thu lai.");}
        }
    }
}
