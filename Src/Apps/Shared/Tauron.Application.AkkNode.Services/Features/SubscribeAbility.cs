using System;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.Features
{
    public sealed class SubscribeFeature

    [PublicAPI]
    public sealed class SubscribeAbility
    {
        private sealed record KeyHint(IActorRef Target, Type Key);

        private readonly ExpandedReceiveActor _actor;

        private IUntypedActorContext ActorContext => ExpandedReceiveActor.ExposedContext;

        public event Action<Terminated>? Terminated;

        public event Action<InternalEventSubscription>? NewSubscription; 

        private GroupDictionary<Type, IActorRef> _sunscriptions = new();

        public SubscribeAbility(ExpandedReceiveActor actor) 
            => _actor = actor;

        public void MakeReceive()
        {
            _actor.WhenReceive<Terminated>(obs => obs.ToUnit(t => Terminated?.Invoke(t)));

            _actor.WhenReceive<KeyHint>(obs => obs.ToUnit(kh => _sunscriptions.Remove(kh.Key, kh.Target)));
            
            _actor.WhenReceive<EventSubscribe>(obs => obs.Select(m => new { Message = m, ExpandedReceiveActor.ExposedContext.Sender, Context = ExpandedReceiveActor.ExposedContext })
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
            
            _actor.WhenReceive<EventUnSubscribe>(obs => obs.Select(msg => new { Message = msg, ExpandedReceiveActor.ExposedContext.Sender })
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