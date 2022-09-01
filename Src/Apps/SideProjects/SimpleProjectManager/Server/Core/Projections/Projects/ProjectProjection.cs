using System.Collections.Immutable;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections;

public sealed class ProjectProjection : IProjectorData<ProjectId>
{
    [BsonId]
    public ProjectId Id { get; set; } = ProjectId.New;

    public ProjectName JobName { get; set; } = ProjectName.Empty;

    public ProjectStatus Status { get; set; }

    public SortOrder Ordering { get; set; } = null!;

    public ProjectDeadline? Deadline { get; set; }

    public ImmutableList<ProjectFileId> ProjectFiles { get; set; } = ImmutableList<ProjectFileId>.Empty;

    public ProjectProjection()
    {
        
    }

    [UsedImplicitly]
    public ProjectProjection(ProjectStatus status, SortOrder ordering, ProjectDeadline? deadline)
    {
        Status = status;
        Ordering = ordering;
        Deadline = deadline;
    }
}