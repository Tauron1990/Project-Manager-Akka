using System.Collections.Immutable;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Data.States;

public sealed record WriteCriticalError(DateTime Occurrence, string ApplicationPart, string Message, string? StackTrace, ImmutableList<ErrorProperty> ContextData)
{
    public CriticalError ToCriticalError() => new(string.Empty, Occurrence, ApplicationPart, Message, StackTrace, ContextData);
}

public static class ErrorStateRequests
{
    public static Func<WriteCriticalError, CancellationToken, ValueTask<string?>> WriteError(ICriticalErrorService service)
    {
        return async (error, token) =>
               {
                   await service.WriteError(error.ToCriticalError(), token);

                   return null;
               };
    }
}