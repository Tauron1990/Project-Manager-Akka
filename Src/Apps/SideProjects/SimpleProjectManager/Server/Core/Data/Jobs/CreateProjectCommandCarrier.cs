using Akkatecture.Commands;
using JetBrains.Annotations;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed class CreateProjectCommandCarrier : Command<Project, ProjectId>
{
    public CreateProjectCommand Command { get; }

    public CreateProjectCommandCarrier(CreateProjectCommand command) 
        : base(ProjectId.For(command.Project))
        => Command = command;

    public CreateProjectCommandCarrier(CreateProjectCommand command, CommandId sourceId) 
        : base(ProjectId.For(command.Project), sourceId)
        => Command = command;
}