using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

internal static class ErrorStateRequests
{
    internal static Func<WriteCriticalError, CancellationToken, ValueTask<SimpleResult>> WriteError(ICriticalErrorService service)
    {
        return async (error, token) =>
               {
                   await service.WriteError(error.ToCriticalError(), token).ConfigureAwait(false);

                   return SimpleResult.Success();
               };
    }

    internal static Func<DisableError, CancellationToken, ValueTask<SimpleResult>> DisableError(ICriticalErrorService errorService)
        => async (disable, token) => await errorService.DisableError(disable.Error.Id, token).ConfigureAwait(false);
}