using Akkatecture.Commands;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

[UsedImplicitly]
public sealed class DataModule : IModule
{
    private static void ProjectRegistrations(IServiceCollection builder)
    {
        builder.TryAddSingleton(CommandMapping.For<Project, ProjectId, ProjectState, ProjectManager, Command<Project, ProjectId>>());

        builder.TryAddSingleton(ApiCommandMapping.For<CreateProjectCommand>(c => new CreateProjectCommandCarrier(c)));
        builder.TryAddSingleton(ApiCommandMapping.For<UpdateProjectCommand>(c => new UpdateProjectCommandCarrier(c)));
        builder.TryAddSingleton(ApiCommandMapping.For<ProjectAttachFilesCommand>(c => new ProjectAttachFilesCommandCarrier(c)));
        builder.TryAddSingleton(ApiCommandMapping.For<ProjectRemoveFilesCommand>(c => new ProjectRemoveFilesCommandCarrier(c)));
        builder.TryAddSingleton(ApiCommandMapping.For<ProjectId>(i => new ProjectDeleteCommandCarrier(i)));
    }

    public void Load(IServiceCollection collection)
    {
        collection.TryAddSingleton<CommandProcessor>();
        
        ProjectRegistrations(collection);
    }
}