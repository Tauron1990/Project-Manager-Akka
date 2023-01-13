using SimpleProjectManager.Client.Shared.Data.States.Data;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

internal static class ErrorStatePatcher
{
    internal static InternalErrorState DecrementErrorCount(InternalErrorState errorState, DisableError error)
    {
        if(errorState.ErrorCount > 0)
            return errorState with { ErrorCount = errorState.ErrorCount - 1 };

        return errorState;
    }
}