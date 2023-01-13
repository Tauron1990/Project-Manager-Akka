using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

#pragma warning disable MA0095
#pragma warning disable CA1067
public sealed class FileName : SingleValueObject<string>, IEquatable<FileName>, IComparable<FileName>
    #pragma warning restore CA1067
    #pragma warning restore MA0095
{
    public static readonly FileName Empty = new(string.Empty);
    public FileName(string value) : base(value) { }

    public int CompareTo(FileName? other)
        => string.Compare(Value, other?.Value, StringComparison.Ordinal);

    public bool Equals(FileName? other)
        => Value.Equals(other?.Value, StringComparison.Ordinal);
}