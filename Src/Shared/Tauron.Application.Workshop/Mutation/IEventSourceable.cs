using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Mutation
{
    [PublicAPI]
    public interface IEventSourceable<out TData>
    {
        IEventSource<TRespond>
            EventSource<TRespond>(Func<TData, TRespond> transformer, Func<TData, bool>? where = null);

        IEventSource<TRespond> EventSource<TRespond>(Func<IObservable<TData>, IObservable<TRespond>> transform);
    }
}