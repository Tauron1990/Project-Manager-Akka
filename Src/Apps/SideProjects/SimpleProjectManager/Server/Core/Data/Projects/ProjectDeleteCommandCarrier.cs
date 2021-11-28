using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class ProjectDeleteCommandCarrier : CommandCarrier<ProjectId, Project, ProjectId>
{
    public ProjectDeleteCommandCarrier(ProjectId command) : base(command, command) { }
}