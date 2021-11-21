using Autofac;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TaskManagerCore>().SingleInstance();
        base.Load(builder);
    }
}