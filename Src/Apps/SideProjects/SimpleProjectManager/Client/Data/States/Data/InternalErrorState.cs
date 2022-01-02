using System.Collections.Immutable;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Data.States;

public sealed record InternalErrorState(int ErrorCount, ImmutableList<CriticalError> SuppliedErrors)
{
    public InternalErrorState()
        : this(0, ImmutableList<CriticalError>.Empty){ }

    public bool Equals(InternalErrorState? other)
    {
        if (other is null) return false;

        return ErrorCount == other.ErrorCount && SuppliedErrors.SequenceEqual(other.SuppliedErrors);
    }

    public override int GetHashCode()
        => HashCode.Combine(ErrorCount, SuppliedErrors.Aggregate(0, (i, pair) => HashCode.Combine(i, pair.GetHashCode())));
}