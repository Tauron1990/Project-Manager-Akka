using Akka.Actor;
using Autofac;

namespace SimpleProjectManager.Server.Core.Projections;

public class ProjectionModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ProjectProjectionManager>().SingleInstance().OnActivated(arg => arg.Instance.Initialize(arg.Context.Resolve<ActorSystem>()));
        base.Load(builder);
    }
}