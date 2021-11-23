using Autofac;
using SimpleProjectManager.Server.Core.JobManager;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

public sealed class MainModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterStartUpAction<ClusterJoinSelf>();
        builder.RegisterStartUpAction<JobManagerRegistrations>();
        base.Load(builder);
    }
}