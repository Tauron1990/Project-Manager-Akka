using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using SimpleProjectManager.Client.Core;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using Tauron.Application;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat;
using ReactiveUI;
using SimpleProjectManager.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(new BaseUrl(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

#if DEBUG
builder.Services.AddLogging(b =>
                            {
                                b.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information)
                                   .AddBrowserConsole();
                            });
#endif

ConfigFusion(builder.Services, new Uri(builder.HostEnvironment.BaseAddress));




ServiceRegistrar.RegisterServices(builder.Services);

var host = builder.Build();

host.Services.UseMicrosoftDependencyResolver();

#if DEBUG
Console.WriteLine("Debug Delay");
await Task.Delay(1000);
#endif
await host.RunAsync();



static void ConfigFusion(IServiceCollection collection, Uri baseAdress)
{
    Console.WriteLine($"Base Adress: {baseAdress}");

    collection.AddSingleton<BlazorModeHelper>();
    collection.AddFusion()
       .AddFusionTime()
       .AddBlazorUIServices()
       .AddRestEaseClient(((_, options) =>
                           {
                               options.BaseUri = baseAdress;
                               options.IsLoggingEnabled = true;
                           }))
       .ConfigureHttpClientFactory(
            (_, _, options) => options.HttpClientActions.Add(
                c =>
                {
                    Console.WriteLine($"Client Config: {c.BaseAddress} -- {baseAdress}");
                    c.BaseAddress = baseAdress;
                }))
       .AddReplicaService<IJobDatabaseService, IJobDatabaseServiceDef>()
       .AddReplicaService<IJobFileService, IJobFileServiceDef>();
}