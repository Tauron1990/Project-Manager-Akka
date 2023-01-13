using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class ProjectAttachFilesCommandCarrier : CommandCarrier<ProjectAttachFilesCommand, Project, ProjectId>
{
    public ProjectAttachFilesCommandCarrier(ProjectAttachFilesCommand command)
        : base(command, command.Id) { }
}