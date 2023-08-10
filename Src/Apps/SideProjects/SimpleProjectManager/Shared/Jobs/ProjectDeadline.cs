using System.Runtime.Serialization;
using Akkatecture.ValueObjects;
using MemoryPack;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable]
#pragma warning disable MA0097
public sealed partial class ProjectDeadline : SingleValueObject<DateTime>
    #pragma warning restore MA0097
{
    public ProjectDeadline(DateTime value) : base(value) { }


    public static ProjectDeadline? FromDateTime(in DateTime? dateTime)
        => dateTime is null ? null : new ProjectDeadline(dateTime.Value.ToUniversalTime());
}