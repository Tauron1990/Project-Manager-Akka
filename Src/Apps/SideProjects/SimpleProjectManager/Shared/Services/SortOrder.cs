namespace SimpleProjectManager.Shared.Services;

public sealed record SortOrder(ProjectId Id, int SkipCount, bool IsPriority)
{
    public SortOrder WithCount(int count)
        => this with { SkipCount = count };

    public SortOrder Increment()
        => this with { SkipCount = SkipCount + 1 };

    public SortOrder Decrement()
        => this with { SkipCount = SkipCount - 1 };

    public SortOrder Priority()
        => this with { IsPriority = true };
}