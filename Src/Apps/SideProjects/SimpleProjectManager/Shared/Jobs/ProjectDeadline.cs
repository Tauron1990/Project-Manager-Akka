using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

#pragma warning disable MA0097
public sealed class ProjectDeadline : SingleValueObject<DateTime>
    #pragma warning restore MA0097
{
    public ProjectDeadline(DateTime value) : base(value) { }


    public static ProjectDeadline? FromDateTime(in DateTime? dateTime)
        => dateTime is null ? null : new ProjectDeadline(dateTime.Value.ToUniversalTime());
}