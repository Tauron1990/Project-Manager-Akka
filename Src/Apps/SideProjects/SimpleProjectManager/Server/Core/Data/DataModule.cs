using Akkatecture.Commands;
using Autofac;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandProcessor>().SingleInstance();

        builder.RegisterInstance(CommandMapping.For<Project, ProjectId, ProjectState, ProjectManager, Command<Project, ProjectId>>());
        builder.RegisterInstance(ApiCommandMapping.For<CreateProjectCommand>(c => new CreateProjectCommandCarrier(c)));

        base.Load(builder);
    }
}