using JetBrains.Annotations;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections;

[UsedImplicitly]
public sealed record ProjectProjection : IProjectorData<ProjectId>
{
    public ProjectName JobName { get; set; } = ProjectName.Empty;

    public ProjectStatus Status { get; set; }

    public SortOrder Ordering { get; set; } = SortOrder.Empty;

    public ProjectDeadline? Deadline { get; set; }

    public List<ProjectFileId> ProjectFiles { get; set; } = new();
    public ProjectId Id { get; set; } = ProjectId.New;
}