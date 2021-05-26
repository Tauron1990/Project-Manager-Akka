using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.DependencyInjection;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Core
{
    public sealed class WorkspaceSuperviserActor : ObservableActor
    {
        private ImmutableDictionary<IActorRef, Action> _intrest = ImmutableDictionary<IActorRef, Action>.Empty;
        private readonly Random _random = new();
        private readonly Dictionary<string, SupervisorStrategy> _supervisorStrategies = new();

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

                if(obj.SupervisorStrategy != null)
                    _supervisorStrategies.Add(name, obj.SupervisorStrategy);

                props = obj.Props(Context);
                var newActor = Context.ActorOf(props, name);

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

        protected override SupervisorStrategy SupervisorStrategy()
            => new WorkspaceSupervisorStrategy(
                new OneForOneStrategy(
                    Decider.From(Directive.Restart,
                        Directive.Stop.When<ActorInitializationException>(),
                        Directive.Stop.When<ActorKilledException>(),
                        Directive.Stop.When<DeathPactException>())), _supervisorStrategies);

        private sealed class WorkspaceSupervisorStrategy : SupervisorStrategy
        {
            private readonly SupervisorStrategy _def;
            private readonly Dictionary<string, SupervisorStrategy> _custom;

            public WorkspaceSupervisorStrategy(SupervisorStrategy def, Dictionary<string, SupervisorStrategy> custom)
            {
                _def = def;
                _custom = custom;
            }

            protected override Directive Handle(IActorRef child, Exception exception) => Get(child).Decider.Decide(exception);

            public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children) 
                => Get(child).ProcessFailure(context, restart, child, cause, stats, children);

            public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children) 
                => Get(child).HandleChildTerminated(actorContext, child, children);

            public override ISurrogate ToSurrogate(ActorSystem system) => throw new NotSupportedException(nameof(WorkspaceSupervisorStrategy));

            public override IDecider Decider => _def.Decider;

            private SupervisorStrategy Get(IActorRef child) => _custom.TryGetValue(child.Path.Name, out var sup) ? sup : _def;
        }

        internal abstract class SuperviseActorBase
        {
            protected SuperviseActorBase(string name) => Name = name;

            public virtual SupervisorStrategy? SupervisorStrategy { get; } = null;

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

            public override Func<IUntypedActorContext, Props> Props => c => ServiceProvider.For(c.System).Props(_actorType);
        }

        internal sealed class CustomSuperviseActor : SuperviseActorBase
        {
            public CustomSuperviseActor([NotNull] string name, Func<IUntypedActorContext, Props> props, SupervisorStrategy? supervisorStrategy) : base(name)
            {
                Props = props;
                SupervisorStrategy = supervisorStrategy;
            }

            public override SupervisorStrategy? SupervisorStrategy { get; }

            public override Func<IUntypedActorContext, Props> Props { get; }
        }

        internal sealed class NewActor
        {
            public NewActor(IActorRef actorRef) => ActorRef = actorRef;

            public IActorRef ActorRef { get; }
        }
    }
}