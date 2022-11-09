using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Util;
using Tauron.Application.Akka.Redux.Extensions;

namespace Tauron.Application.Akka.Redux.Internal;

public sealed class Store<TState> : IReduxStore<TState>
{
    private readonly ISourceQueueWithComplete<object> _actionDispatcher;

    private readonly Source<DispatchedAction<TState>, NotUsed> _actions;

    private readonly Sink<object, NotUsed> _actionSink;

    private readonly SharedKillSwitch _claseFlows = KillSwitches.Shared("Disposing");
    private readonly TState _initialState;
    private readonly Action<Exception> _onError;
    private readonly HashSet<Type> _registratedActions = new();
    private readonly Source<TState, NotUsed> _states;
    private readonly Sink<TState, NotUsed> _stateSink;

    public Store(TState initialState, Action<Exception> onError, IMaterializer materializer)
    {
        _initialState = initialState;
        CurrentState = initialState;
        _onError = onError;
        Materializer = materializer;

        var actionDispatcher = Source.Queue<object>(10, OverflowStrategy.Backpressure);
        var dispatcher = MergeHub.Source<object>();

        var (actualDispatcher, actions) = dispatcher.PreMaterialize(materializer);
        _actionDispatcher = actionDispatcher.Via(_claseFlows.Flow<object>()).ToMaterialized(actualDispatcher, Keep.Left).Run(materializer);
        _actionSink = actualDispatcher;

        var (stateCollector, stateDispatcher) = MergeHub.Source<TState>().PreMaterialize(materializer);
        _states = stateDispatcher
           .Via(_claseFlows.Flow<TState>())
           .ToMaterialized(BroadcastHub.Sink<TState>(), Keep.Right)
           .Run(materializer);

        _stateSink = stateCollector;

        _actions = Source.FromGraph(
                GraphDsl.Create(
                    b =>
                    {
                        var combiner = b.Add(LastStateShape.Create<TState, object, DispatchedAction<TState>>((s, a) => new DispatchedAction<TState>(s, a)));

                        b.From(Source.Single(_initialState).Concat(_states)).To(combiner.In0);
                        b.From(actions.Where(a => a is not null)).To(combiner.In1);

                        return new SourceShape<DispatchedAction<TState>>(combiner.Out);
                    }))
           .Via(_claseFlows.Flow<DispatchedAction<TState>>())
           .ToMaterialized(BroadcastHub.Sink<DispatchedAction<TState>>(), Keep.Right)
           .Run(materializer);

        _states.Via(_claseFlows.Flow<TState>()).RunForeach(s => CurrentState = s, materializer);

        MutateCallbackPlugin.Install(this);
    }

    public IMaterializer Materializer { get; }

    public bool CanProcess<TAction>()
        => CanProcess(typeof(TAction));

    public bool CanProcess(Type type)
        => _registratedActions.Contains(type);

    public Source<TAction, NotUsed> ObservAction<TAction>() where TAction : class
        => from dispatchedAction in _actions
           where dispatchedAction.Action is TAction
           select (TAction)dispatchedAction.Action;

    public Task<IQueueOfferResult> Dispatch(object action)
        => _actionDispatcher.OfferAsync(action);

    public Sink<object, NotUsed> Dispatcher()
        => _actionSink;

    public void Dispose()
        => _claseFlows.Shutdown();

    public Source<TResult, NotUsed> Select<TResult>(Flow<TState, TResult, NotUsed> selector)
        => _states.Via(selector);

    public Source<TState, NotUsed> Select()
        => _states;

    public Source<ActionState<TState, TAction>, NotUsed> ObservActionState<TAction>()
        => from dispatchedActions in _actions
           where dispatchedActions.Action is TAction
           select new ActionState<TState, TAction>(dispatchedActions.State, (TAction)dispatchedActions.Action);

    public Source<TResult, NotUsed> ObservAction<TAction, TResult>(Flow<(TState State, TAction Action), TResult, NotUsed> selector)
        => (
            from dispatchedAction in _actions
            where dispatchedAction.Action is TAction
            select (dispatchedAction.State, (TAction)dispatchedAction.Action)
        ).Via(selector);

    public TState CurrentState { get; private set; }

    public void Reset()
        => _stateSink.RunWith(Source.Single(_initialState), Materializer);

    public void RegisterReducers(IEnumerable<On<TState>> reducers)
        => reducers.Foreach(RegisterReducer);

    public void RegisterEffects(IEnumerable<Effect<TState>> effects)
        => effects.Foreach(RegisterEffect);

    public void RegisterReducers(params On<TState>[] reducers)
        => reducers.Foreach(RegisterReducer);

    public void RegisterEffects(params Effect<TState>[] effects)
        => effects.Foreach(RegisterEffect);

    public Source<object, NotUsed> ObserveAction()
        => from dispatchedAction in _actions
           where dispatchedAction.Action is not null
           select dispatchedAction.Action;

    public Source<TAction, NotUsed> ObserveAction<TAction>() where TAction : class
        => from dispatchedAction in _actions
           where dispatchedAction.Action is TAction
           select (TAction)dispatchedAction.Action;

    public Source<TResult, NotUsed> ObserveAction<TAction, TResult>(Flow<(TAction Action, TState State), TResult, NotUsed> resultSelector) where TAction : class
        =>
        (
            from dispatchedAction in _actions
            where dispatchedAction.Action is TAction
            select ((TAction)dispatchedAction.Action, dispatchedAction.State)
        ).Via(resultSelector);

    private void RegisterReducer(On<TState> reducer)
    {
        if(!_registratedActions.Add(reducer.ActionType))
            throw new InvalidOperationException("The ActionType is already Registrated");

        _actions
           .Via(_claseFlows.Flow<DispatchedAction<TState>>())
           .Via(
                RestartFlow.OnFailuresWithBackoff(
                    () => reducer.Mutator.Recover(
                        ex =>
                        {
                            _onError(ex);

                            return Option<TState>.None;
                        }),
                    RestartSettings.Create(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), 1)))
           .RunWith(_stateSink, Materializer);
    }

    private void RegisterEffect(Effect<TState> effect)
    {
        var effectSource = RestartSource.OnFailuresWithBackoff(
            () => effect.CreateEffect(this).Recover(
                ex =>
                {
                    _onError(ex);

                    return Option<object>.None;
                }),
            RestartSettings.Create(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), 1));

        _actionSink.RunWith(effectSource.Via(_claseFlows.Flow<object>()), Materializer);
    }
}