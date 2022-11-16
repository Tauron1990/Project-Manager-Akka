using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.Data.States.Data;

public sealed record InternalErrorState(ErrorCount ErrorCount)
{
    public InternalErrorState()
        : this(ErrorCount.From(0)) { }
}