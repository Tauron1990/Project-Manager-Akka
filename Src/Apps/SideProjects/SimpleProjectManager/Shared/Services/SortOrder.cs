using Akkatecture.Entities;

namespace SimpleProjectManager.Shared.Services;

public sealed class SortOrder : Entity<ProjectId>
{
    public int SkipCount { get; }

    public bool IsPriority { get; }
    
    public SortOrder(ProjectId id, int skipCount, bool isPriority) : base(id)
    {
        SkipCount = skipCount;
        IsPriority = isPriority;
    }

    public SortOrder WithCount(int count)
        => new(Id, count, IsPriority);
    
    public SortOrder Increment()
        => new(Id, SkipCount + 1, IsPriority);
    
    public SortOrder Decrement()
        => new(Id, SkipCount - 1, IsPriority);

    public SortOrder Priority()
        => IsPriority ? this : new SortOrder(Id, 0, true);
}