using ReMarket.Infrastructure.Exchange;

namespace ReMarket.Api.Authentication;

public sealed class HetHanTraoDoiWorker(IServiceScopeFactory scopes,ILogger<HetHanTraoDoiWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30),token);
                await using var scope=scopes.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<TraoDoiService>().HetHanAsync(token);
            }
            catch(OperationCanceledException)when(token.IsCancellationRequested){break;}
            catch(Exception ex){logger.LogWarning(ex,"Chua giai phong duoc de nghi trao doi het han; se thu lai.");}
        }
    }
}
