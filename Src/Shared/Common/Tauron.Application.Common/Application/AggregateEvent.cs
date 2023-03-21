using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tauron.Application;

[PublicAPI]
public class AggregateEvent<TPayload> : IDisposable
{
    private readonly Subject<TPayload> _handlerList = new();

    public void Dispose() => 
        _handlerList.Dispose();

    public virtual void Publish(TPayload content) =>
        _handlerList.OnNext(content);

    [Pure]
    public IObservable<TPayload> Get() => 
        _handlerList.AsObservable();
}