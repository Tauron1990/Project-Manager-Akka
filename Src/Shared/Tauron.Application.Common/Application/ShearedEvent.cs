using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [PublicAPI]
    public class SharedEvent<TPayload> : IDisposable
    {
        private Subject<TPayload>? _handlerList = new();

        public virtual void Publish(TPayload content) 
            => _handlerList?.OnNext(content);

        public IObservable<TPayload> Get() 
            => _handlerList?.AsObservable() ?? Observable.Empty<TPayload>();

        public void Dispose()
        {
            var list = Interlocked.Exchange(ref _handlerList, null);
            list?.OnCompleted();
            list?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    [PublicAPI]
    public interface IEventAggregator
    {
        TEventType GetEvent<TEventType, TPayload>() where TEventType : SharedEvent<TPayload>, new();
    }

    [PublicAPI]
    public sealed class EventAggregator : IEventAggregator, IDisposable
    {
        private readonly Dictionary<Type, IDisposable> _events = new();

        public TEventType GetEvent<TEventType, TPayload>() where TEventType : SharedEvent<TPayload>, new()
        {
            var t = typeof(TEventType);
            if (!_events.ContainsKey(t)) _events[t] = new TEventType();

            return (TEventType) _events[t];
        }

        public void Dispose()
        {
            _events.Values.Foreach(d => d.Dispose());
            _events.Clear();
        }
    }
}