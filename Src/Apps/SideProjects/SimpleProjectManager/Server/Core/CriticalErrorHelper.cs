using System.Collections.Immutable;
using System.Diagnostics;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core;

public sealed class CriticalErrorHelper
{
    private readonly string _generalPart;
    private readonly ICriticalErrorService _service;
    public ILogger Logger { get; }

    public CriticalErrorHelper(string generalPart, ICriticalErrorService service, ILogger logger)
    {
        _generalPart = generalPart;
        _service = service;
        Logger = logger;
    }

    public async ValueTask<IOperationResult> WriteError(string detailPart, Exception exception, Func<ImmutableList<ErrorProperty>>? erros = null)
    {
        var part = $"{_generalPart} -- {detailPart}";
        try
        {
            await _service.WriteError(
                new CriticalError(
                    string.Empty,
                    DateTime.UtcNow,
                    part,
                    exception.Message,
                    exception.Demystify().StackTrace ?? string.Empty,
                    erros?.Invoke() ?? ImmutableList<ErrorProperty>.Empty),
                default);
                
            return OperationResult.Failure(exception);
        }
        catch (Exception serviceException)
        {
            if(serviceException is OperationCanceledException)
                return OperationResult.Failure(exception);
                
            Logger.LogError(serviceException, "Error on Write Critical Error for {Part} with {Message}", part, exception.Message);
                
            return OperationResult.Failure(exception);
        }
    }

    public async ValueTask<IOperationResult> Try(string detailPart, Func<ValueTask<IOperationResult>> runner, CancellationToken token, Func<ImmutableList<ErrorProperty>>? erros = null)
    {
        try
        {
            return await runner();
        }
        catch (Exception e)
        {
            if(e is OperationCanceledException) 
                return OperationResult.Failure(e);

            return await WriteError(detailPart, e, erros);
        }
    }
}