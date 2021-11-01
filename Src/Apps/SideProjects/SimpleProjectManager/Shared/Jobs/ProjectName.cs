using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class ProjectName : SingleValueObject<string>
{
    public ProjectName(string value) : base(value) { }

    public static readonly ProjectName Empty = new(string.Empty);
}