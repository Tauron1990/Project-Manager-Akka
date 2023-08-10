using System.Runtime.Serialization;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record SortOrder
{
    public static readonly SortOrder Empty = new();

    [property:DataMember, MemoryPackOrder(0)]
    public ProjectId Id { get; init; } = ProjectId.Empty;

    [property:DataMember, MemoryPackOrder(1)]
    public int SkipCount { get; init; }

    [property:DataMember, MemoryPackOrder(2)]
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