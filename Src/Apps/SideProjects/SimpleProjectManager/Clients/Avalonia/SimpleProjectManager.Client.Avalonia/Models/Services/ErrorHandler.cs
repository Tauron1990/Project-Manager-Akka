using System;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Application;

namespace SimpleProjectManager.Client.Avalonia.Models.Services;

public sealed class ErrorHandler : IErrorHandler
{
    private readonly IEventAggregator _eventAggregator;

    public ErrorHandler(IEventAggregator eventAggregator)
        => _eventAggregator = eventAggregator;

    public void RequestError(string error)
        => _eventAggregator.PublishSharedMessage(SharedMessage.CreateError($"Request Fehler: {error}"));

    public void RequestError(Exception error)
        => RequestError(error.Message);

    public void StateDbError(Exception error)
        => _eventAggregator.PublishSharedMessage(SharedMessage.CreateError($"Datenbank Error: {error.Message}"));

    public void TimeoutError(Exception error)
        => _eventAggregator.PublishSharedMessage(SharedMessage.CreateError($"Timeout Fheler: {error.Message}"));

    public void StoreError(Exception error)
    {
        _eventAggregator.PublishSharedMessage(SharedMessage.CreateError($"Store Error: {error}"));
    }
}