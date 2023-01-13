using System.Collections.Immutable;
using Akkatecture.Aggregates.Snapshot;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public sealed record ProjectStateSnapshot(ProjectName Name, ImmutableList<ProjectFileId> Files, ProjectDeadline? Deadline, ProjectStatus Status)
    : IAggregateSnapshot<Project, ProjectId>;