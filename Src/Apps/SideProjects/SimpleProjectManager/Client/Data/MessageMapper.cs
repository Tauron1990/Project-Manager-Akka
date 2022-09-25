using System.Reactive;
using SimpleProjectManager.Client.Shared.Services;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data;

public sealed class MessageDispatcher : IMessageDispatcher
{
    private readonly IEventAggregator _eventAggregator;

    public MessageDispatcher(IEventAggregator eventAggregator)
        => _eventAggregator = eventAggregator;

    public Func<TInput, bool> IsSuccess<TInput>(Func<TInput, string> runner)
        => _eventAggregator.IsSuccess(runner);

    public ValueTask<bool> IsSuccess(Func<ValueTask<string>> runner)
        => _eventAggregator.IsSuccess(runner);

    public ValueTask<bool> IsSuccess(Func<ValueTask<Unit>> runner)
        => _eventAggregator.IsSuccess(runner);

    public ValueTask<bool> IsSuccess(Func<ValueTask> runner)
        => _eventAggregator.IsSuccess(runner);

    public void PublishError(Exception exception)
        => _eventAggregator.PublishError(exception);

    public void PublishError(string error)
        => _eventAggregator.PublishError(error);

    public void PublishWarnig(string warning)
        => _eventAggregator.PublishWarnig(warning);

    public void PublishMessage(string message)
        => _eventAggregator.PublishInfo(message);
}