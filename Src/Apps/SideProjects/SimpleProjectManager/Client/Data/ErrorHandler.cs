using System.Diagnostics;
using Tauron.Applicarion.Redux;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data;

public class ErrorHandler : IErrorHandler
{
    private readonly IEventAggregator _eventAggregator;

    public ErrorHandler(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public void RequestError(string error)
    {
        PrintError(nameof(RequestError), error);
        _eventAggregator.PublishError(error);
    }

    public void RequestError(Exception error)
    {
        PrintError(nameof(RequestError), error);
        _eventAggregator.PublishError(error);
    }

    public void StateDbError(Exception error)
    {
        PrintError(nameof(StateDbError), error);
        _eventAggregator.PublishWarnig(error);
    }

    public void TimeoutError(Exception error)
    {
        PrintError(nameof(TimeoutError), error);
        _eventAggregator.PublishWarnig(error);
    }

    public void StoreError(Exception error)
    {
        PrintError(nameof(StoreError), error);
        _eventAggregator.PublishError(error);
    }

    [Conditional("DEBUG")]
    private static void PrintError(string name, object toPrint)
        => Console.WriteLine($"{nameof(ErrorHandler)} -- {name} .. {toPrint}");
}