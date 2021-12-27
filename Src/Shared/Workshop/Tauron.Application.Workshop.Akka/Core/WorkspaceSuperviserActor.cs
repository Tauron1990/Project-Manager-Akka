using System.Collections.Immutable;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.DependencyInjection;
using Akka.Util;
using Tauron.TAkka;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.Core;

public sealed class WorkspaceSuperviserActor : ObservableActor
{
    private readonly Random _random = new();
    private readonly Dictionary<string, SupervisorStrategy> _supervisorStrategies = new();
    private ImmutableDictionary<IActorRef, Action> _intrest = ImmutableDictionary<IActorRef, Action>.Empty;

    public WorkspaceSuperviserActor()
    {
        Receive<SuperviseActorBase>(obs => obs.SubscribeWithStatus(CreateActor));

        Receive<WatchIntrest>(
            obs => obs.SubscribeWithStatus(
                wi =>
                {
                    ImmutableInterlocked.AddOrUpdate(
                        ref _intrest, 
                        wi.Target, _ => wi.OnRemove, 
                        (_, action) => action.Combine(wi.OnRemove) ?? wi.OnRemove);
                    Context.Watch(wi.Target);
                }));
        Receive<Terminated>(
            obs => obs.SubscribeWithStatus(
                t =>
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

            if (obj.SupervisorStrategy != null)
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
                Decider.From(
                    Directive.Restart,
                    Directive.Stop.When<ActorInitializationException>(),
                    Directive.Stop.When<ActorKilledException>(),
                    Directive.Stop.When<DeathPactException>())),
            _supervisorStrategies);

    private sealed class WorkspaceSupervisorStrategy : SupervisorStrategy
    {
        private readonly Dictionary<string, SupervisorStrategy> _custom;
        private readonly SupervisorStrategy _def;

        internal WorkspaceSupervisorStrategy(SupervisorStrategy def, Dictionary<string, SupervisorStrategy> custom)
        {
            _def = def;
            _custom = custom;
        }

        public override IDecider Decider => _def.Decider;

        protected override Directive Handle(IActorRef child, Exception exception) => Get(child).Decider.Decide(exception);

        public override void ProcessFailure(IActorContext context, bool restart, IActorRef child, Exception cause, ChildRestartStats stats, IReadOnlyCollection<ChildRestartStats> children)
            => Get(child).ProcessFailure(context, restart, child, cause, stats, children);

        public override void HandleChildTerminated(IActorContext actorContext, IActorRef child, IEnumerable<IInternalActorRef> children)
            => Get(child).HandleChildTerminated(actorContext, child, children);

        public override ISurrogate ToSurrogate(ActorSystem system) => throw new NotSupportedException(nameof(WorkspaceSupervisorStrategy));

        private SupervisorStrategy Get(IActorRef child) => _custom.TryGetValue(child.Path.Name, out var sup) ? sup : _def;
    }

    internal abstract class SuperviseActorBase
    {
        protected SuperviseActorBase(string name) => Name = name;

        internal virtual SupervisorStrategy? SupervisorStrategy { get; } = null;

        internal abstract Func<IUntypedActorContext, Props> Props { get; }

        internal string Name { get; }
    }

    internal sealed class SupervisePropsActor : SuperviseActorBase
    {
        internal SupervisePropsActor(Props props, string name)
            : base(name)
        {
            Props = _ => props;
        }

        internal override Func<IUntypedActorContext, Props> Props { get; }
    }

    internal sealed class SuperviseDiActor : SuperviseActorBase
    {
        private readonly Type _actorType;

        internal SuperviseDiActor(Type actorType, string name) : base(name) => _actorType = actorType;

        internal override Func<IUntypedActorContext, Props> Props => c => DependencyResolver.For(c.System).Props(_actorType);
    }

    internal sealed class CustomSuperviseActor : SuperviseActorBase
    {
        internal CustomSuperviseActor(string name, Func<IUntypedActorContext, Props> props, SupervisorStrategy? supervisorStrategy) : base(name)
        {
            Props = props;
            SupervisorStrategy = supervisorStrategy;
        }

        internal override SupervisorStrategy? SupervisorStrategy { get; }

        internal override Func<IUntypedActorContext, Props> Props { get; }
    }

    internal sealed class NewActor
    {
        internal NewActor(IActorRef actorRef) => ActorRef = actorRef;

        internal IActorRef ActorRef { get; }
    }
}