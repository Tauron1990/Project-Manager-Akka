using System;
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