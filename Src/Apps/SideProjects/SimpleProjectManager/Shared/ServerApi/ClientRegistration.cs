using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Shared.ServerApi.RestApi;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Devices;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;

namespace SimpleProjectManager.Shared.ServerApi;

public static class ClientRegistration
{
    public static FusionBuilder ConfigFusion(IServiceCollection collection, Uri baseAdress)
    {
        return collection.AddFusion()
            .AddFusionTime()
           .AddRestEaseClient(
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
                });
    }
}