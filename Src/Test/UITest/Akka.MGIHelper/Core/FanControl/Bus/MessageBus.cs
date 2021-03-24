using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Tauron.Features;

namespace Akka.MGIHelper.Core.FanControl.Bus
{
    public sealed class MessageBus : IDisposable
    {
        private readonly Dictionary<Type, List<object>> _handlers = new();
        private readonly object _lock = new();

        public void Dispose()
        {
            foreach (var dis in _handlers.SelectMany(l => l.Value).OfType<IDisposable>())
                dis.Dispose();

            _handlers.Clear();
        }

        public void Subscribe<TMsg>(IHandler<TMsg> handler)
        {
            var msgType = typeof(TMsg);

            if (_handlers.TryGetValue(msgType, out var list))
                list.Add(handler);
            else
                _handlers.TryAdd(msgType, new List<object> {handler});
        }

        //public async Task<Unit> Publish<TMsg>(TMsg msg)
        //{
        //        foreach (var handler in _handlers[typeof(TMsg)].OfType<IHandler<TMsg>>()) await handler.Handle(msg, this).ConfigureAwait(false);
        //        return Unit.Default;
        //}

        public async Task<TState> Publish<TState, TMsg>(StatePair<TMsg, TState> msg)
        {
            foreach (var handler in _handlers[typeof(TMsg)].OfType<IHandler<TMsg>>()) await handler.Handle(msg.State, this).ConfigureAwait(false);
            return msg.State;
        }

        //private sealed class SubscribeDispose<TMsg> : IDisposable
        //{
        //    private readonly IHandler<TMsg> _handler;
        //    private readonly List<object> _handlers;
        //    private readonly object _locker;

        //    public SubscribeDispose(IHandler<TMsg> handler, List<object> handlers, object locker)
        //    {
        //        _handler = handler;
        //        _handlers = handlers;
        //        _locker = locker;
        //    }

        //    public void Dispose()
        //    {
        //        lock (_locker)
        //        {
        //            _handlers.Remove(_handler);
        //        }
        //    }
        //}
    }
}