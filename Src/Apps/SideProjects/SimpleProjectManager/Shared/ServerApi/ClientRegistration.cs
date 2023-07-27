using Microsoft.Extensions.DependencyInjection;
using RestEase;
using SimpleProjectManager.Shared.ServerApi.RestApi;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Devices;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using Stl.Rpc.Infrastructure;

namespace SimpleProjectManager.Shared.ServerApi;

public static class ClientRegistration
{
    public static FusionBuilder ConfigFusion(IServiceCollection collection, Uri baseAdress)
    {
        FusionBuilder fusion = collection
            .AddSingleton(_=> new RestClient(baseAdress))
            .AddSingleton(sp => sp.GetRequiredService<RestClient>().For<IJobDatabaseServiceDef>())
            .AddSingleton(sp => sp.GetRequiredService<RestClient>().For<IJobFileServiceDef>())
            .AddSingleton(sp => sp.GetRequiredService<RestClient>().For<ICriticalErrorServiceDef>())
            .AddSingleton(sp => sp.GetRequiredService<RestClient>().For<ITaskManagerDef>())
            .AddSingleton(sp => sp.GetRequiredService<RestClient>().For<IDeviceServiceDef>())
            .AddHttpClient()

            .AddFusion()
#pragma warning disable EPS06
            .AddFusionTime()
            //.AddRpcPeerStateMonitor()
            .AddClient<IJobDatabaseService>()
            .AddClient<IJobFileService>()
            .AddClient<ICriticalErrorService>()
            .AddClient<ITaskManager>()
            .AddClient<IDeviceService>()
           /*.AddRestEaseClient(
                b =>
                {
                    b.ConfigureWebSocketChannel(_ => new WebSocketChannelProvider.Options { BaseUri = baseAdress });
                    b.ConfigureHttpClient(
                            (_, _, options) =>
                            {
                                options.HttpClientActions.Add(
                                    c => { c.BaseAddress = baseAdress; });
                            })
                       .AddReplicaService<IJobDatabaseService, IJobDatabaseServiceDef>()
                       .AddReplicaService<IJobFileService, IJobFileServiceDef>()
                       .AddReplicaService<ICriticalErrorService, ICriticalErrorServiceDef>()
                       .AddReplicaService<ITaskManager, ITaskManagerDef>()
                       .AddReplicaService<IDeviceService, IDeviceServiceDef>();
                })*/;

           fusion.Rpc.AddWebSocketClient(baseAdress);

           return fusion;
    }
}