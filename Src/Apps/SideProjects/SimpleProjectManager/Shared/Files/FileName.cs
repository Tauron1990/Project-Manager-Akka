using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class FileName : SingleValueObject<string>
{
    public static readonly FileName Empty = new(string.Empty);
    public FileName(string value) : base(value) { }
}