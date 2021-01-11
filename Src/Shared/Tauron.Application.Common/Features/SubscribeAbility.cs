using System;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application;

namespace Tauron.Features
{

    [PublicAPI]
    public sealed class SubscribeAbility
    {
        private sealed record KeyHint(IActorRef Target, Type Key);

        private readonly ObservableActor _actor;

        private IUntypedActorContext ActorContext => ObservableActor.ExposedContext;

        public event Action<Terminated>? Terminated;

        public event Action<InternalEventSubscription>? NewSubscription; 

        private GroupDictionary<Type, IActorRef> _sunscriptions = new();

        public SubscribeAbility(ObservableActor actor) 
            => _actor = actor;

        public void MakeReceive()
        {
            _actor.Receive<Terminated>(obs => obs.ToUnit(t => Terminated?.Invoke(t)));

            _actor.Receive<KeyHint>(obs => obs.ToUnit(kh => _sunscriptions.Remove(kh.Key, kh.Target)));
            
            _actor.Receive<EventSubscribe>(obs => obs.Select(m => new { Message = m, ObservableActor.ExposedContext.Sender, Context = ObservableActor.ExposedContext })
                                                         .Where(d => !d.Sender.IsNobody())
                                                         .Do(d =>
                                                             {
                                                                 if (d.Message.Watch)
                                                                     d.Context.WatchWith(d.Sender, new KeyHint(d.Sender, d.Message.Event));
                                                             })
                                                         .Select(d =>
                                                                 {
                                                                     _sunscriptions.Add(d.Message.Event, d.Sender);
                                                                     return new InternalEventSubscription(d.Sender, d.Message.Event);
                                                                 })
                                                         .ToUnit(evt => NewSubscription?.Invoke(evt)));
            
            _actor.Receive<EventUnSubscribe>(obs => obs.Select(msg => new { Message = msg, ObservableActor.ExposedContext.Sender })
                                                           .Where(d => !d.Sender.IsNobody())
                                                           .ToUnit(d =>
                                                                   {
                                                                       ActorContext.Unwatch(d.Sender);
                                                                       _sunscriptions.Remove(d.Message.Event, d.Sender);
                                                                   }));
        }

        public TType Send<TType>(TType payload)
        {
            if (!_sunscriptions.TryGetValue(typeof(TType), out var coll)) return payload;
            
            foreach (var actorRef in coll) actorRef.Tell(payload);
            return payload;
        }

        public TType Send<TType>(TType payload, Type evtType)
        {
            if (!_sunscriptions.TryGetValue(evtType, out var coll)) return payload;

            foreach (var actorRef in coll) actorRef.Tell(payload);
            return payload;
        }

        public sealed record InternalEventSubscription(IActorRef Intrest, Type Type);
    }

    [PublicAPI]
    public sealed record EventUnSubscribe(Type Event);

    [PublicAPI]
    public sealed record EventSubscribe(bool Watch, Type Event);
}