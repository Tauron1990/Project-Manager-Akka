using Hyperion.Internal;
using SimpleProjectManager.Server.Controllers.FileUpload;
using SimpleProjectManager.Server.Core.JobManager;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

[UsedImplicitly]
public sealed class MainModule : IModule
{
    public void Load(IServiceCollection collection)
    {
        collection.RegisterStartUpAction<ClusterJoinSelf>();
        collection.RegisterStartUpAction<JobManagerRegistrations>();

        collection.AddScoped<FileUploadTransaction>();
    }
}