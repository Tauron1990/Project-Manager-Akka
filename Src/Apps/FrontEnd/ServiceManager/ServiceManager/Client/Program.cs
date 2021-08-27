using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Reactive.PlatformServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using MudBlazor.Services;
using ServiceManager.Client.ServiceDefs;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.Identity;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;

namespace ServiceManager.Client
{
    public class Program
    {
        private sealed class RecconectionPolicy : IRetryPolicy
        {
            public TimeSpan? NextRetryDelay(RetryContext retryContext)
                => retryContext.PreviousRetryCount switch
                {
                    0 => TimeSpan.FromSeconds(1),
                    1 => TimeSpan.FromSeconds(10),
                    200 => null,
                    _ => TimeSpan.FromSeconds(30)
                };
        }

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            //builder.RootComponents.Add<App>("#app");
            builder.Services.AddMudServices(o => o.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter);
            builder.Services.AddScoped(sp => new HubConnectionBuilder()
                                            .WithAutomaticReconnect(new RecconectionPolicy())
                                            .WithUrl(sp.GetRequiredService<NavigationManager>().ToAbsoluteUri("/ClusterInfoHub"))
                                            .Build());

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            
            ConfigFusion(builder.Services, new Uri(builder.HostEnvironment.BaseAddress));
            ServiceConfiguration.Run(builder.Services);

            await builder.Build().RunAsync();
        }

        private static void ConfigFusion(IServiceCollection collection, Uri baseAdress)
        {
            collection.AddSingleton<BlazorModeHelper>();
            collection.AddFusion()
                      .AddFusionTime()
                      .AddBlazorUIServices()
                      .AddAuthentication(
                           o => o.AddBlazor().AddRestEaseClient())
                      .AddRestEaseClient()
                      .ConfigureHttpClientFactory((_, _, options) => options.HttpClientActions.Add(c => c.BaseAddress = baseAdress))
                      .AddClientService<IClusterNodeTracking, IClusterNodeTrackingDef>()
                      .AddClientService<IClusterConnectionTracker, IClusterConnectionTrackerDef>()
                      .AddClientService<IServerInfo, IServerInfoDef>()
                      .AddClientService<IAppIpManager, IAppIpManagerDef>()
                      .AddClientService<IDatabaseConfig, IDatabaseConfigDef>()
                      .AddClientService<IServerConfigurationApi, IServerConfigurationApiDef>()
                      .AddClientService<IUserManagement, IUserManagementDef>();
        }
    }
}
