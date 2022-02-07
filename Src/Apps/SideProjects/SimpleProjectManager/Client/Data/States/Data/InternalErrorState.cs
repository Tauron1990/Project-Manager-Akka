namespace SimpleProjectManager.Client.Data.States;

public sealed record InternalErrorState(long ErrorCount)
{
    public InternalErrorState()
        : this(0){ }
}