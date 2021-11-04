using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SimpleProjectManager.Client.Core;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using Tauron.Application;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(new BaseUrl(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

ConfigFusion(builder.Services, new Uri(builder.HostEnvironment.BaseAddress));
RegisterServices();

await builder.Build().RunAsync();

void RegisterServices()
{
    builder.Services.AddMudServices();
    builder.Services.AddSingleton<IEventAggregator, EventAggregator>();
}

static void ConfigFusion(IServiceCollection collection, Uri baseAdress)
{
    collection.AddSingleton<BlazorModeHelper>();
    collection.AddFusion()
       .AddFusionTime()
       .AddBlazorUIServices()
       .AddRestEaseClient()
       .ConfigureHttpClientFactory(
            (_, _, options) => options.HttpClientActions.Add(
                c =>
                {
                    Console.WriteLine($"Client Config: {c.BaseAddress} -- {baseAdress}");
                    c.BaseAddress = baseAdress;
                }))
       .ConfigureWebSocketChannel(new WebSocketChannelProvider.Options { BaseUri = baseAdress })
       .AddClientService<IJobDatabaseService, IJobDatabaseServiceDef>()
       .AddClientService<IJobFileService, IJobFileServiceDef>();
}