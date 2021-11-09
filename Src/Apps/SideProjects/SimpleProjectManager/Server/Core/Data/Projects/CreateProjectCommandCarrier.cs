using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class CreateProjectCommandCarrier : CommandCarrier<CreateProjectCommand, Project, ProjectId>
{

    public CreateProjectCommandCarrier(CreateProjectCommand data) 
        : base(data, ProjectId.For(data.Project)){}
}