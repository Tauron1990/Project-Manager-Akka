using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class ProjectDeadline : SingleValueObject<DateTimeOffset>
{
    public ProjectDeadline(DateTimeOffset value) : base(value) { }
}