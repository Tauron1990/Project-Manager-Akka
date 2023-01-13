using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Tauron.Application;

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
        Type eventType = typeof(TEventType);
        if(!_events.ContainsKey(eventType)) _events[eventType] = new TEventType();

        return (TEventType)_events[eventType];
    }
}