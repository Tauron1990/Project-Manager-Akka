using Akkatecture.Entities;

namespace SimpleProjectManager.Shared.Services;

public sealed class SortOrder : Entity<ProjectId>
{
    public int SkipCount { get; }

    public SortOrder(ProjectId id, int skipCount) : base(id)
        => SkipCount = skipCount;

    public SortOrder WithCount(int count)
        => new(Id, count);
}