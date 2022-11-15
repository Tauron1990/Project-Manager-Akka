using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Data.Data;

public sealed record DbProjectProjection
{
    public string Id { get; set; } = string.Empty;

    public string JobName { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }

    public DbSortOrder Ordering { get; set; } = DbSortOrder.Empty;

    public DateTime? Deadline { get; set; }

    public IList<string> ProjectFiles { get; set; } = new List<string>();
}