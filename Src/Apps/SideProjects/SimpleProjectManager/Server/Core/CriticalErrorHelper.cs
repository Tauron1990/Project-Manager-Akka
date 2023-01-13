using System.Collections.Immutable;
using System.Diagnostics;
using JetBrains.Annotations;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Server.Core;

[PublicAPI]
public sealed class CriticalErrorHelper
{
    private readonly string _generalPart;
    private readonly ICriticalErrorService _service;

    public CriticalErrorHelper(string generalPart, ICriticalErrorService service, ILogger logger)
    {
        _generalPart = generalPart;
        _service = service;
        Logger = logger;
    }

    public ILogger Logger { get; }

    public async ValueTask<SimpleResult> WriteError(string detailPart, Exception exception, Func<ImmutableList<ErrorProperty>>? erros = null)
    {
        PropertyValue part = PropertyValue.From($"{_generalPart} -- {detailPart}");
        try
        {
            await _service.WriteError(
                new CriticalError(
                    ErrorId.New,
                    DateTime.UtcNow,
                    part,
                    SimpleMessage.From(exception.Message),
                    StackTraceData.FromException(exception.Demystify()),
                    erros?.Invoke() ?? ImmutableList<ErrorProperty>.Empty),
                default).ConfigureAwait(false);

            return SimpleResult.Failure(exception);
        }
        catch (Exception serviceException)
        {
            if(serviceException is OperationCanceledException)
                return SimpleResult.Failure(exception);

            Logger.LogError(serviceException, "Error on Write Critical Error for {Part} with {Message}", part, exception.Message);

            return SimpleResult.Failure(exception);
        }
    }

    public async ValueTask<SimpleResult> Try(string detailPart, Func<ValueTask<SimpleResult>> runner, CancellationToken token, Func<ImmutableList<ErrorProperty>>? erros = null)
    {
        try
        {
            return await runner().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            if(e is OperationCanceledException)
                return SimpleResult.Failure(e);

            return await WriteError(detailPart, e, erros).ConfigureAwait(false);
        }
    }

    public async ValueTask<SimpleResult> ProcessTransaction(TransactionResult result, string detailePart, Func<ImmutableList<ErrorProperty>> propertys)
    {
        (TrasnactionState trasnactionState, Exception? exception) = result;

        if(trasnactionState == TrasnactionState.Successeded) return SimpleResult.Success();
        if(exception is null)
            return SimpleResult.Failure("Unbekannter Fehler");

        switch (trasnactionState)
        {
            case TrasnactionState.RollbackFailed:
                await WriteError(
                    detailePart,
                    exception,
                    propertys).ConfigureAwait(false);

                return SimpleResult.Failure(exception.Message);
            default:
                return SimpleResult.Failure(exception.Message);
        }
    }
}