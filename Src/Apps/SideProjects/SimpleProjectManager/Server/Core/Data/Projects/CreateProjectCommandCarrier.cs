using Akkatecture.Commands;
using JetBrains.Annotations;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class CreateProjectCommandCarrier : Command<Project, ProjectId>
{
    public CreateProjectCommand Data { get; }

    public CreateProjectCommandCarrier(CreateProjectCommand data) 
        : base(ProjectId.For(data.Project))
        => Data = data;

    public CreateProjectCommandCarrier(CreateProjectCommand data, CommandId sourceId) 
        : base(ProjectId.For(data.Project), sourceId)
        => Data = data;
}