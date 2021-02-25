using System;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.DI.Core;
using Tauron.Akka;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Core
{
    public sealed class WorkspaceSuperviserActor : ObservableActor
    {
        private ImmutableDictionary<IActorRef, Action> _intrest = ImmutableDictionary<IActorRef, Action>.Empty;
        private Random _random = new();

        public WorkspaceSuperviserActor()
        {
            Receive<SuperviseActorBase>(obs => obs.SubscribeWithStatus(CreateActor));

            Receive<WatchIntrest>(obs => obs.SubscribeWithStatus(wi =>
            {
                ImmutableInterlocked.AddOrUpdate(ref _intrest, wi.Target, _ => wi.OnRemove, (_, action) => action.Combine(wi.OnRemove) ?? wi.OnRemove);
                Context.Watch(wi.Target);
            }));
            Receive<Terminated>(obs => obs.SubscribeWithStatus(t =>
            {
                if (!_intrest.TryGetValue(t.ActorRef, out var action)) return;

                action();
                _intrest = _intrest.Remove(t.ActorRef);
            }));
        }

        private void CreateActor(SuperviseActorBase obj)
        {
            Props? props = null;

            try
            {
                string name = obj.Name;
                while (!Context.Child(name).Equals(ActorRefs.Nobody)) 
                    name = obj.Name + "-" + _random.Next();

                props = obj.Props(Context);
                var newActor = Context.ActorOf(props, obj.Name);

                if (Sender.IsNobody()) return;
                Sender.Tell(new NewActor(newActor));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Create an new Actor {TypeName}", props?.TypeName ?? "Unkowen");

                if (Sender.IsNobody()) return;
                Sender.Tell(new NewActor(ActorRefs.Nobody));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy() => new OneForOneStrategy(
            Decider.From(Directive.Resume,
                Directive.Stop.When<ActorInitializationException>(),
                Directive.Stop.When<ActorKilledException>(),
                Directive.Stop.When<DeathPactException>()));

        internal abstract class SuperviseActorBase
        {
            protected SuperviseActorBase(string name) => Name = name;

            public abstract Func<IUntypedActorContext, Props> Props { get; }

            public string Name { get; }
        }

        internal sealed class SupervisePropsActor : SuperviseActorBase
        {
            public SupervisePropsActor(Props props, string name)
                : base(name)
            {
                Props = _ => props;
            }

            public override Func<IUntypedActorContext, Props> Props { get; }
        }

        internal sealed class SuperviseDiActor : SuperviseActorBase
        {
            private readonly Type _actorType;

            public SuperviseDiActor(Type actorType, string name) : base(name) => _actorType = actorType;

            public override Func<IUntypedActorContext, Props> Props => c => c.System.DI().Props(_actorType);
        }

        internal sealed class NewActor
        {
            public NewActor(IActorRef actorRef) => ActorRef = actorRef;

            public IActorRef ActorRef { get; }
        }
    }
}