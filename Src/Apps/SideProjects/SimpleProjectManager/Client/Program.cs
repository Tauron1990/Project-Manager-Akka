using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleProjectManager.Client;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.ServerApi.RestApi;
using Splat.Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Blazor;
using Tauron;
using Tauron.Application;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(new BaseUrl(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

#if DEBUG
builder.Services.AddLogging(
    b =>
    {
        b.SetMinimumLevel(LogLevel.Debug);
        b.AddBrowserConsole();
    });
#endif

//Fusion
builder.Services.AddSingleton<BlazorModeHelper>();
//builder.Services.AddSingleton<IUICommandTracker, UICommandTracker>();
FusionBuilder config = ClientRegistration.ConfigFusion(builder.Services, new Uri(builder.HostEnvironment.BaseAddress));
config.AddBlazorUIServices();

//Services
ServiceRegistrar.RegisterServices(builder.Services);
builder.Services.RegisterModule<InternalDataModule>();


WebAssemblyHost host = builder.Build();

host.Services.UseMicrosoftDependencyResolver();
TauronEnviromentSetup.Run(host.Services);

await host.RunAsync().ConfigureAwait(false);

Console.WriteLine("Application Closed");