using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Shared.ServerApi.RestApi;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;

namespace SimpleProjectManager.Shared.ServerApi;

public static class ClientRegistration
{
    public static FusionRestEaseClientBuilder ConfigFusion(IServiceCollection collection, Uri baseAdress)
    {
        return collection.AddFusion()
           .AddFusionTime()
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
           .AddReplicaService<IJobFileService, IJobFileServiceDef>()
           .AddReplicaService<ICriticalErrorService, ICriticalErrorServiceDef>()
           .AddReplicaService<ITaskManager, ITaskManagerDef>();
    }
}