using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data;

public class ErrorHandler : IErrorHandler
{
    private readonly IEventAggregator _eventAggregator;

    public ErrorHandler(IEventAggregator eventAggregator)
        => _eventAggregator = eventAggregator;

    public void RequestError(string error)
        => _eventAggregator.PublishError(error);

    public void RequestError(Exception error)
        => _eventAggregator.PublishError(error);

    public void StateDbError(Exception error)
        => _eventAggregator.PublishWarnig(error);

    public void TimeoutError(Exception error)
        => _eventAggregator.PublishWarnig(error);

    public void StoreError(Exception error)
        => _eventAggregator.PublishError(error);
}