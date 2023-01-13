namespace SimpleProjectManager.Server.Data.Data;

public sealed record DbSortOrder
{
    public static readonly DbSortOrder Empty = new();

    public string Id { get; init; } = string.Empty;

    public int SkipCount { get; init; }

    public bool IsPriority { get; init; }
}