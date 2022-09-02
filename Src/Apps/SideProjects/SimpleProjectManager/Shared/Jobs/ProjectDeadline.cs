using Akkatecture.ValueObjects;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared;

public sealed class ProjectDeadline : SingleValueObject<DateTime>
{
    public ProjectDeadline(DateTime value) : base(value) { }

    [UsedImplicitly, Obsolete("Used for Serialization")]
    public ProjectDeadline()
    {
        
    }
    
    public static ProjectDeadline? FromDateTime(DateTime? dateTime)
        => dateTime == null ? null : new ProjectDeadline(dateTime.Value.ToUniversalTime());
}