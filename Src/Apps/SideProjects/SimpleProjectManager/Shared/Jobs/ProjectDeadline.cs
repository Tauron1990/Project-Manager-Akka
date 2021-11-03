using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class ProjectDeadline : SingleValueObject<DateTimeOffset>
{
    public ProjectDeadline(DateTimeOffset value) : base(value) { }

    public static ProjectDeadline? FromDateTime(DateTime? dateTime)
        => dateTime == null ? null : new ProjectDeadline(dateTime.Value);
}