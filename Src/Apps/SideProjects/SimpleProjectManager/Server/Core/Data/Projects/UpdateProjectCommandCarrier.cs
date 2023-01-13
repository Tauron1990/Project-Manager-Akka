using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class UpdateProjectCommandCarrier : CommandCarrier<UpdateProjectCommand, Project, ProjectId>
{
    public UpdateProjectCommandCarrier(UpdateProjectCommand command)
        : base(command, command.Id) { }
}