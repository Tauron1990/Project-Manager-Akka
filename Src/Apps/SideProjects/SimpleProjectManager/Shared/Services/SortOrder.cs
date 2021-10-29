using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared.Services;

public sealed class SortOrder : SingleValueObject<long>
{
    public SortOrder(long value) : base(value) { }
}