using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReMarket.Api.Controllers;

namespace ReMarket.DatabaseChecks;

// Test-runner instrumentation only. Does not record bodies, headers, tokens, emails or concrete resource IDs.
internal sealed class DoPhuApi : IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int,int>> observations=new();
    private static DoPhuApi? current;
    public DoPhuApi()=>current=this;
    public sealed class Filter:IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)=>app=>{
            app.Use(async(context,run)=>{try{await run(context);}finally{current?.Record(context);}});next(app);
        };
    }
    public sealed class ActionCapture:IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
        {context.HttpContext.Items[typeof(DoPhuApi)]=context.HttpContext.GetEndpoint();await next();}
    }
    private void Record(HttpContext context)
    {
        context.Items.TryGetValue(typeof(DoPhuApi),out var captured);
        if((context.GetEndpoint()??captured as Endpoint) is not RouteEndpoint endpoint)return;
        var path="/"+endpoint.RoutePattern.RawText?.TrimStart('/');
        path=System.Text.RegularExpressions.Regex.Replace(path,@"\{([^}:]+)(?::[^}]+)?\}","{$1}");
        var statuses=observations.GetOrAdd(context.Request.Method+" "+path,_=>new());
        statuses.AddOrUpdate(context.Response.StatusCode,1,(_,n)=>n+1);
    }
    public void Dispose(){if(current==this)current=null;}
    private sealed class InventoryFactory:WebApplicationFactory<QuanTriController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder b)=>b.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","..","src","ReMarket.Api")))
            .UseEnvironment("Development").ObserveApi().ConfigureLogging(x=>x.ClearProviders()).ConfigureAppConfiguration((_,c)=>c.AddInMemoryCollection(new Dictionary<string,string?>{
                ["Jwt:SigningKey"]=Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),["ConnectionStrings:ReMarket"]="Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True"}));
    }
    public async Task ExportAsync(string file)
    {
        await using var factory=new InventoryFactory();using var client=factory.CreateClient();
        using var swagger=JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        var rows=new List<object>();var covered=0;var successful=0;
        foreach(var p in swagger.RootElement.GetProperty("paths").EnumerateObject())foreach(var m in p.Value.EnumerateObject())
        {
            if(m.Name is not("get" or "post" or "put" or "delete" or "patch"))continue;
            var key=m.Name.ToUpperInvariant()+" "+p.Name;
            var statuses=observations.GetValueOrDefault(key)??new();
            if(!statuses.IsEmpty)covered++;if(statuses.Keys.Any(x=>x is >=200 and <300))successful++;
            rows.Add(new{method=m.Name.ToUpperInvariant(),path=p.Name,statuses=statuses.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value),observed=!statuses.IsEmpty,success=statuses.Keys.Any(x=>x is >=200 and <300)});
        }
        var result=new{generatedUtc=DateTimeOffset.UtcNow,total=rows.Count,observed=covered,withSuccessfulResponse=successful,endpoints=rows};
        var path=Path.GetFullPath(file);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path,JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"API coverage: {covered}/{rows.Count} routes observed; {successful} with a2xx response. Unobserved routes are not declared tested.");
    }
}
internal static class DoPhuApiExtensions
{
    public static IWebHostBuilder ObserveApi(this IWebHostBuilder builder)=>builder.ConfigureServices(s=>{s.AddSingleton<IStartupFilter,DoPhuApi.Filter>();s.Configure<MvcOptions>(o=>o.Filters.Add<DoPhuApi.ActionCapture>());});
}
