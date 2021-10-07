using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public class AggregateEvent<TPayload> : IDisposable
{
    private Subject<TPayload>? _handlerList = new();

    public void Dispose()
    {
        var list = Interlocked.Exchange(ref _handlerList, null);
        list?.OnCompleted();
        list?.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual void Publish(TPayload content)
        => _handlerList?.OnNext(content);

    public IObservable<TPayload> Get()
        => _handlerList?.AsObservable() ?? Observable.Empty<TPayload>();
}

[PublicAPI]
public interface IEventAggregator
{
    TEventType GetEvent<TEventType, TPayload>() where TEventType : AggregateEvent<TPayload>, new();
}

[PublicAPI]
public sealed class EventAggregator : IEventAggregator, IDisposable
{
    private readonly Dictionary<Type, IDisposable> _events = new();

    public void Dispose()
    {
        _events.Values.Foreach(disposable => disposable.Dispose());
        _events.Clear();
    }

    public TEventType GetEvent<TEventType, TPayload>() where TEventType : AggregateEvent<TPayload>, new()
    {
        var eventType = typeof(TEventType);
        if (!_events.ContainsKey(eventType)) _events[eventType] = new TEventType();

        return (TEventType)_events[eventType];
    }
}