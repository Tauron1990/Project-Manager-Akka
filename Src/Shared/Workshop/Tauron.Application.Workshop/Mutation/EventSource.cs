using System;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation;

[PublicAPI]
public sealed class EventSource<TRespond, TData> : EventSourceBase<TRespond>
{
    public EventSource(
        Func<TData, TRespond> transform,
        Func<TData, bool>? where, IObservable<TData> handler)
    {
        if(where is null)
            handler.Select(transform).Subscribe(Sender());
        else
            handler.Where(where).Select(transform).Subscribe(Sender());
    }

    public EventSource(IObservable<TRespond> handler)
        => handler.Subscribe(Sender());
}