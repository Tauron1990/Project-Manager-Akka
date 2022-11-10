using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

public sealed record DisableError(CriticalError Error);

public sealed record WriteCriticalError(DateTime Occurrence, PropertyValue ApplicationPart, SimpleMessage Message, StackTraceData? StackTrace, ImmutableList<ErrorProperty> ContextData)
{
    public CriticalError ToCriticalError() => new(ErrorId.New, Occurrence, ApplicationPart, Message, StackTrace, ContextData);
}

internal static class ErrorStatePatcher
{
    public static InternalErrorState DecrementErrorCount(InternalErrorState errorState, DisableError error)
    {
        if(errorState.ErrorCount > 0)
            return errorState with { ErrorCount = errorState.ErrorCount - 1 };

        return errorState;
    }
}

internal static class ErrorStateRequests
{
    public static Func<WriteCriticalError, CancellationToken, ValueTask<SimpleResult>> WriteError(ICriticalErrorService service)
    {
        return async (error, token) =>
               {
                   await service.WriteError(error.ToCriticalError(), token);

                   return SimpleResult.Success();
               };
    }

    public static Func<DisableError, CancellationToken, ValueTask<SimpleResult>> DisableError(ICriticalErrorService errorService)
        => async (disable, token) => await errorService.DisableError(disable.Error.Id, token);
}