using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.TAkka;
using Tauron.Application.Workshop.Mutation;
using Tauron.Operations;

namespace Tauron.Application.CommonUI.Model;

[PublicAPI]
public abstract class ActorModel : ObservableActor
{
    private readonly Dictionary<Type, Action<IOperationResult>> _compledActions = new();

    protected ActorModel(IActionInvoker actionInvoker)
    {
        ActionInvoker = actionInvoker;
        Receive<IOperationResult>(obs => obs.SubscribeWithStatus(OnOperationCompled));
    }

    public IActionInvoker ActionInvoker { get; }

    protected virtual void OnOperationCompled(IOperationResult result)
    {
        var actionType = result.Outcome?.GetType();

        if (actionType?.IsAssignableTo(typeof(IStateAction)) != true) return;

        if (_compledActions.TryGetValue(actionType, out var action))
            action(result);
    }

    public void WhenActionComnpled<TAction>(Action<IOperationResult> opsAction)
        where TAction : IStateAction
    {
        var key = typeof(TAction);
        _compledActions[key] = opsAction.Combine(_compledActions.GetValueOrDefault(key))!;
    }

    public UIStateConfiguration<TState> WhenStateChanges<TState>(string? name = null)
        where TState : class
        => new(
            ActionInvoker.GetState<TState>(name ?? string.Empty) ??
            throw new ArgumentException("No such State Found"),
            this);

    public void DispatchAction(IStateAction action, bool? sendBack = true)
        => ActionInvoker.Run(action, sendBack);

    [PublicAPI]
    public sealed class UIStateConfiguration<TState>
    {
        private readonly ActorModel _actor;
        private readonly TState _state;

        public UIStateConfiguration(TState state, ActorModel actor)
        {
            _state = state;
            _actor = actor;
        }

        public UIStateEventConfiguration<TEvent> FromEvent<TEvent>(
            Func<TState, IEventSource<TEvent>> source,
            Action<UIStateEventConfiguration<TEvent>>? configAction = null)
        {
            var config = new UIStateEventConfiguration<TEvent>(source(_state), _actor);
            configAction?.Invoke(config);

            return config;
        }
    }

    [PublicAPI]
    public sealed class UIStateEventConfiguration<TEvent>
    {
        private readonly ActorModel _actor;
        private readonly IEventSource<TEvent> _eventSource;

        public UIStateEventConfiguration(IEventSource<TEvent> eventSource, ActorModel actor)
        {
            _eventSource = eventSource;
            _actor = actor;
        }

        public IObservable<TEvent> ToObservable() => _eventSource.ObserveOn(ActorScheduler.From(_actor.Self));
    }
}