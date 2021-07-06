using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [PublicAPI]
    public class SharedEvent<TPayload> : IDisposable
    {
        private readonly Subject<TPayload> _handlerList = new();

        public virtual void Publish(TPayload content) 
            => _handlerList.OnNext(content);

        public IObservable<TPayload> Get() 
            => _handlerList.AsObservable();

        public void Dispose()
        {
            _handlerList.OnCompleted();
            _handlerList.Dispose();
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