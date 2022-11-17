using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class ProjectRemoveFilesCommandCarrier : CommandCarrier<ProjectRemoveFilesCommand, Project, ProjectId>
{
    public ProjectRemoveFilesCommandCarrier(ProjectRemoveFilesCommand command)
        : base(command, command.Id) { }
}