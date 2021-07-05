using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor.Services;

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
            #if(DEBUG)
            await Task.Delay(6000);
            #endif

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddMudServices();
            builder.Services.AddScoped(sp => new HubConnectionBuilder()
                                            .WithAutomaticReconnect(new RecconectionPolicy())
                                            .WithUrl(sp.GetRequiredService<NavigationManager>().ToAbsoluteUri("/ClusterInfoHub"))
                                            .Build());
            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            ServiceConfiguration.Run(builder.Services);

            await builder.Build().RunAsync();
        }
    }
}
