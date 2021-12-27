using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Features;
using Tauron.Operations;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public abstract class ActionInvokerActorFeature<TClassState> : ActorFeatureBase<TClassState>
{
    private readonly Dictionary<Type, Action<IOperationResult>> _compledActions = new();

    protected ActionInvokerActorFeature(IActionInvoker actionInvoker)
    {
        ActionInvoker = actionInvoker;
        Receive<IOperationResult>(obs => obs.Select(d => d.Event).SubscribeWithStatus(InternalOnOperationCompled));
    }

    public IActionInvoker ActionInvoker { get; }

    private void InternalOnOperationCompled(IOperationResult result)
    {
        var actionType = result.Outcome?.GetType();

        if (actionType?.IsAssignableTo(typeof(IStateAction)) != true) return;

        if (_compledActions.TryGetValue(actionType, out var action))
        {
            action(result);

            return;
        }

        OnOperationCompled(result);
    }

    protected virtual void OnOperationCompled(IOperationResult result) { }

    public TState GetState<TState>(string key = "") where TState : class => ActionInvoker.GetState<TState>(key) ??
                                                                            throw new InvalidOperationException("No such State Found");

    public void ConfigurateState<TState>(Action<TState> toConfig)
        where TState : class
        => ConfigurateState(string.Empty, toConfig);

    public void ConfigurateState<TState>(string key, Action<TState> toConfig) where TState : class
    {
        var state = GetState<TState>(key);
        toConfig(state);
    }

    public void WhenActionComnpled<TAction>(Action<IOperationResult> opsAction)
        where TAction : IStateAction
    {
        var key = typeof(TAction);
        _compledActions[key] = opsAction.Combine(_compledActions.GetValueOrDefault(key))!;
    }

    public StateConfig<TState> WhenStateChanges<TState>(string? name = null)
        where TState : class
        => new(
            ActionInvoker.GetState<TState>(name ?? string.Empty) ??
            throw new ArgumentException("No such State Found"),
            this);

    public void DispatchAction(IStateAction action, bool? sendBack = true) => ActionInvoker.Run(action, sendBack);

    [PublicAPI]
    public sealed class StateConfig<TState>
    {
        private readonly TState _state;
        private ActionInvokerActorFeature<TClassState> _actor;

        public StateConfig(TState state, ActionInvokerActorFeature<TClassState> actor)
        {
            _state = state;
            _actor = actor;
        }

        public void Receive<TEvent>(Func<IObservable<Group<TEvent, TState>>, IObservable<Unit>> handler)
            => _actor.Receive<TEvent>(obs => handler(obs.Select(p => new Group<TEvent, TState>(p, _state))));

        public void Receive<TEvent>(Func<IObservable<Group<TEvent, TState>>, IObservable<TClassState>> handler)
            => _actor.Receive<TEvent>(obs => handler(obs.Select(p => new Group<TEvent, TState>(p, _state))));

        public void Receive<TEvent>(
            Func<IObservable<Group<TEvent, TState>>, IObservable<Unit>> handler,
            Func<Exception, bool> errorHandler)
            => _actor.Receive<TEvent>(obs => handler(obs.Select(p => new Group<TEvent, TState>(p, _state))), errorHandler);

        public void Receive<TEvent>(Func<IObservable<Group<TEvent, TState>>, IDisposable> handler)
            => _actor.Receive<TEvent>(obs => handler(obs.Select(p => new Group<TEvent, TState>(p, _state))));
    }

    [PublicAPI]
    public sealed record Group<TEvent, TState>(TEvent Event, TState State, TClassState ClassState, ITimerScheduler Timers)
    {
        public Group(StatePair<TEvent, TClassState> pair, TState state)
            : this(pair.Event, state, pair.State, pair.Timers) { }

        public void Deconstruct(out TEvent evt, out TState state)
        {
            evt = Event;
            state = State;
        }
    }
}