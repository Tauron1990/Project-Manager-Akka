namespace SimpleProjectManager.Shared.Services;

public sealed record SortOrder
{
    public static SortOrder Empty = new();
    
    public ProjectId Id { get; init; } = ProjectId.Empty;

    public int SkipCount { get; init; }
    
    public bool IsPriority { get; init; }
     
    public SortOrder WithCount(int count)
        => this with { SkipCount = count };

    public SortOrder Increment()
        => this with { SkipCount = SkipCount + 1 };

    public SortOrder Decrement()
        => this with { SkipCount = SkipCount - 1 };

    public SortOrder Priority()
        => this with { IsPriority = true };
}