using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class ProjectDeadline : SingleValueObject<DateTime>
{
    public ProjectDeadline(DateTime value) : base(value) { }


    public static ProjectDeadline? FromDateTime(DateTime? dateTime)
        => dateTime == null ? null : new ProjectDeadline(dateTime.Value.ToUniversalTime());
}