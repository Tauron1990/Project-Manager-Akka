using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Data.Data;

public sealed record DbProjectProjection
{
    public string Id { get; set; } = string.Empty;

    public string JobName { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }

    public DBSortOrder Ordering { get; set; } = DBSortOrder.Empty;

    public DateTime? Deadline { get; set; }

    public List<string> ProjectFiles { get; set; } = new();
}

public sealed record DBSortOrder
{
    public static DBSortOrder Empty = new();

    public string Id { get; init; } = string.Empty;

    public int SkipCount { get; init; }

    public bool IsPriority { get; init; }
}