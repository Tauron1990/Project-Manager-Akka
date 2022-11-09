using System.Collections.Immutable;
using Akkatecture.Aggregates;
using Akkatecture.Aggregates.Snapshot;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed record ProjectStateSnapshot(ProjectName Name, ImmutableList<ProjectFileId> Files, ProjectDeadline? Deadline, ProjectStatus Status)
    : IAggregateSnapshot<Project, ProjectId>;

public sealed class ProjectState : InternalState<Project, ProjectId, ProjectStateSnapshot>,
    IApply<NewProjectCreatedEvent>, IApply<ProjectNameChangedEvent>, IApply<ProjectDeadLineChangedEvent>, IApply<ProjectDeletedEvent>,
    IApply<ProjectFilesAttachedEvent>, IApply<ProjectStatusChangedEvent>, IApply<ProjectFilesRemovedEvent>
{
    public ProjectName ProjectName { get; private set; } = ProjectName.Empty;

    public ImmutableList<ProjectFileId> Files { get; private set; } = ImmutableList<ProjectFileId>.Empty;

    public ProjectDeadline? Deadline { get; private set; }

    public ProjectStatus Status { get; private set; }

    public void Apply(NewProjectCreatedEvent aggregateEvent)
        => ProjectName = aggregateEvent.Name;

    public void Apply(ProjectDeadLineChangedEvent aggregateEvent)
        => Deadline = aggregateEvent.Deadline;

    public void Apply(ProjectDeletedEvent aggregateEvent)
    {
        ProjectName = ProjectName.Empty;
        Files = ImmutableList<ProjectFileId>.Empty;
        Deadline = null;
        Status = ProjectStatus.Deleted;
    }

    public void Apply(ProjectFilesAttachedEvent aggregateEvent)
        => Files = Files.AddRange(aggregateEvent.Files);

    public void Apply(ProjectFilesRemovedEvent aggregateEvent)
        => Files = Files.RemoveRange(aggregateEvent.Files);

    public void Apply(ProjectNameChangedEvent aggregateEvent)
        => ProjectName = aggregateEvent.NewName;

    public void Apply(ProjectStatusChangedEvent aggregateEvent)
        => Status = aggregateEvent.NewStatus;

    public override void Hydrate(ProjectStateSnapshot aggregateSnapshot)
    {
        ProjectName = aggregateSnapshot.Name;
        Files = aggregateSnapshot.Files;
        Deadline = aggregateSnapshot.Deadline;
        Status = aggregateSnapshot.Status;
    }

    public override ProjectStateSnapshot CreateSnapshot()
        => new(ProjectName, Files, Deadline, Status);
}