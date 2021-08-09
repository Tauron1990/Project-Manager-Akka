using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using TestWebApplication.Client.Services;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
            var apiBaseUri = new Uri($"{baseUri}api/");

            builder.Services.AddFusion()
                   .AddFusionTime()
                   .AddRestEaseClient(
                        b => b.AddReplicaService<ICounterService, ICounterServiceDef>()
                              .ConfigureHttpClientFactory((c, name, o) =>
                                                          {
                                                              var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                                                              var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                                                              o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
                                                          }))
                   .AddBlazorUIServices();

            builder.Services.AddSingleton<BlazorModeHelper>();

            builder.Services.AddSingleton(sp =>
                                              sp.GetRequiredService<IStateFactory>()
                                                .NewMutable<string>(Guid.NewGuid().ToString("D")));

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            var host = builder.Build();
            await host.Services.HostedServices().Start();
            await host.RunAsync();
        }
    }
}
