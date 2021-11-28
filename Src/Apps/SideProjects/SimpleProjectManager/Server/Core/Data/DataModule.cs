using Akkatecture.Commands;
using Autofac;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class DataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CommandProcessor>().SingleInstance();

        ProjectRegistrations(builder);

        base.Load(builder);
    }

    private static void ProjectRegistrations(ContainerBuilder builder)
    {
        builder.RegisterInstance(CommandMapping.For<Project, ProjectId, ProjectState, ProjectManager, Command<Project, ProjectId>>());

        builder.RegisterInstance(ApiCommandMapping.For<CreateProjectCommand>(c => new CreateProjectCommandCarrier(c)));
        builder.RegisterInstance(ApiCommandMapping.For<UpdateProjectCommand>(c => new UpdateProjectCommandCarrier(c)));
        builder.RegisterInstance(ApiCommandMapping.For<ProjectAttachFilesCommand>(c => new ProjectAttachFilesCommandCarrier(c)));
        builder.RegisterInstance(ApiCommandMapping.For<ProjectRemoveFilesCommand>(c => new ProjectRemoveFilesCommandCarrier(c)));
        builder.RegisterInstance(ApiCommandMapping.For<ProjectId>(i => new ProjectDeleteCommandCarrier(i)));
    }
}