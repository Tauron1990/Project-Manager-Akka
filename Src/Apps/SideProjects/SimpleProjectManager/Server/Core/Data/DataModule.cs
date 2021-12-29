using Akkatecture.Commands;
using JetBrains.Annotations;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

[UsedImplicitly]
public sealed class DataModule : IModule
{
    private static void ProjectRegistrations(IServiceCollection builder)
    {
        builder.AddSingleton(CommandMapping.For<Project, ProjectId, ProjectState, ProjectManager, Command<Project, ProjectId>>());

        builder.AddSingleton(ApiCommandMapping.For<CreateProjectCommand>(c => new CreateProjectCommandCarrier(c)));
        builder.AddSingleton(ApiCommandMapping.For<UpdateProjectCommand>(c => new UpdateProjectCommandCarrier(c)));
        builder.AddSingleton(ApiCommandMapping.For<ProjectAttachFilesCommand>(c => new ProjectAttachFilesCommandCarrier(c)));
        builder.AddSingleton(ApiCommandMapping.For<ProjectRemoveFilesCommand>(c => new ProjectRemoveFilesCommandCarrier(c)));
        builder.AddSingleton(ApiCommandMapping.For<ProjectId>(i => new ProjectDeleteCommandCarrier(i)));
    }

    public void Load(IServiceCollection collection)
    {
        collection.AddSingleton<CommandProcessor>();
        
        ProjectRegistrations(collection);
    }
}