using ReMarket.Infrastructure.QuanTri;
namespace ReMarket.Api.Authentication;
public sealed class MoKhoaHetHanWorker(IServiceScopeFactory scopes,IConfiguration configuration,ILogger<MoKhoaHetHanWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct){if(!configuration.GetValue("Admin:EnableUnlockWorker",true))return;while(!ct.IsCancellationRequested){try{await using var scope=scopes.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<QuanTriService>().MoKhoaHetHanAsync(ct);}catch(OperationCanceledException)when(ct.IsCancellationRequested){break;}catch(Exception ex){logger.LogError(ex,"Không kiểm tra được thời hạn khóa.");}try{await Task.Delay(TimeSpan.FromSeconds(30),ct);}catch(OperationCanceledException)when(ct.IsCancellationRequested){break;}}}
}
