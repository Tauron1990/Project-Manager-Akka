using System.Collections.Immutable;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections;

[UsedImplicitly]
public sealed record ProjectProjection : IProjectorData<ProjectId>
{
    [BsonId]
    public ProjectId Id { get; set; } = ProjectId.New;

    public ProjectName JobName { get; set; } = ProjectName.Empty;

    public ProjectStatus Status { get; set; }

    public SortOrder Ordering { get; set; } = SortOrder.Empty;

    public ProjectDeadline? Deadline { get; set; }

    public ImmutableList<ProjectFileId> ProjectFiles { get; set; } = ImmutableList<ProjectFileId>.Empty;

}