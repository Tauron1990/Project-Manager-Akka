using System.Collections.Immutable;
using MongoDB.Bson.Serialization.Attributes;
using SimpleProjectManager.Shared;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections;

public sealed class ProjectProjection : IProjectorData<ProjectId>
{
    [BsonId]
    public ProjectId Id { get; set; } = ProjectId.New;

    public long LastCheckPoint { get; set; } = 0;

    public ProjectName JobName { get; set; } = new(string.Empty);

    public ProjectStatus Status { get; set; }

    public ProjectDeadline? Deadline { get; set; }

    public ImmutableList<ProjectFileId> ProjectFiles { get; set; } = ImmutableList<ProjectFileId>.Empty;
}