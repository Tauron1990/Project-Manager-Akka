using System.Collections.Immutable;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Data.States;

public sealed record InternalErrorState(long ErrorCount)
{
    public InternalErrorState()
        : this(0){ }
}