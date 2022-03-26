namespace SimpleProjectManager.Client.Shared.Data.States.Data;

public sealed record InternalErrorState(long ErrorCount)
{
    public InternalErrorState()
        : this(0){ }
}