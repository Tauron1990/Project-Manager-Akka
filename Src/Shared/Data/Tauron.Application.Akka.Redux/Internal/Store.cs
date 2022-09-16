using System.Collections.Immutable;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Stage;
using Akka.Util;

namespace Tauron.Application.Akka.Redux.Internal;

public sealed class Store<TState> : IReduxStore<TState>
{
    private sealed class LastState : GraphStage<FanInShape<TState, object, DispatchedAction<TState>>>
    {
        private sealed class Logic : GraphStageLogic
        {
            private readonly LastState _holder;
            private TState? _currentState;
            private ImmutableQueue<object?>? _pending = ImmutableQueue<object?>.Empty;

            public Logic(LastState holder) : base(holder.Shape)
            {
                _holder = holder;

                SetHandler(holder.StateIn, () =>
                                              {
                                                  _currentState = Grab(holder.StateIn);

                                                  if(_pending is not null)
                                                  {
                                                      EmitMultiple(
                                                          _holder.ActionOut,
                                                          Interlocked.Exchange(ref _pending, null)
                                                         .Select(a => new DispatchedAction<TState>(_currentState, a)));
                                                  }
                                                  
                                                  Pull(holder.StateIn);
                                              });
                
                SetHandler(holder.ActionIn,
                    () =>
                    {
                        if(_pending is not null)
                        {
                            ImmutableInterlocked.Enqueue(ref _pending, Grab(holder.ActionIn));
                            Pull(holder.ActionIn);
                            return;
                        }
                        
                        Emit(holder.ActionOut, new DispatchedAction<TState>(_currentState!, Grab(holder.ActionIn)),
                            () => Pull(holder.ActionIn));
                    });
            }

            public override void PreStart()
            {
                Pull(_holder.StateIn);
                Pull(_holder.ActionIn);
            }
        }


        public LastState()
            => Shape = new FanInShape<TState, object, DispatchedAction<TState>>(ActionOut, StateIn, ActionIn);

        private Inlet<TState> StateIn { get; } = new("CombineLast.StateIn");

        private Inlet<object> ActionIn { get; } = new ("CombineLast.ActionIn");

        private Outlet<DispatchedAction<TState>> ActionOut { get; } = new("CombineLast.Out");

        public override FanInShape<TState, object, DispatchedAction<TState>> Shape { get; }

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this);
    }

    private readonly TState _initialState;
    private readonly Action<Exception> _onError;
    private readonly IMaterializer _materializer;
    private readonly HashSet<Type> _registratedActions = new();

    private readonly SharedKillSwitch _claseFlows = KillSwitches.Shared("Disposing");
    
    private readonly ISourceQueueWithComplete<object?> _actionDispatcher;

    private readonly Sink<object?, NotUsed> _actionSink;
    private readonly Sink<TState, NotUsed> _stateSink;

    private readonly Source<DispatchedAction<TState>, NotUsed> _actions;
    private readonly Source<TState, NotUsed> _states;

    public Store(TState initialState, Action<Exception> onError, IMaterializer materializer)
    {
        _initialState = initialState;
        CurrentState = initialState;
        _onError = onError;
        _materializer = materializer;

        var actionDispatcher = Source.Queue<object?>(10, OverflowStrategy.Backpressure);
        var dispatcher = MergeHub.Source<object?>();

        var (actualDispatcher, actions) = dispatcher.PreMaterialize(materializer);
        _actionDispatcher = actionDispatcher.Via(_claseFlows.Flow<object?>()).ToMaterialized(actualDispatcher, Keep.Left).Run(materializer);
        _actionSink = actualDispatcher;

        var (stateCollector, stateDispatcher) = MergeHub.Source<TState>().PreMaterialize(materializer);
        _states = stateDispatcher.Via(_claseFlows.Flow<TState>()).ToMaterialized(BroadcastHub.Sink<TState>(), Keep.Right).Run(materializer);
        _stateSink = stateCollector;

        _actions = Source.FromGraph(
            GraphDsl.Create(
                b =>
                {
                    var combiner = b.Add(new LastState());

                    b.From(Source.Single(_initialState).Concat(_states)).To(combiner.In0);
                    b.From(actions.Where(a => a is not null)).To(combiner.In1);

                    return new SourceShape<DispatchedAction<TState>>(combiner.Out);
                }))
           .Via(_claseFlows.Flow<DispatchedAction<TState>>())
           .ToMaterialized(BroadcastHub.Sink<DispatchedAction<TState>>(), Keep.Right)
           .Run(materializer);

        _states.Via(_claseFlows.Flow<TState>()).RunForeach(s => CurrentState = s, materializer);
    }

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
        => _stateSink.RunWith(Source.Single(_initialState), _materializer);

    private void RegisterReducer(On<TState> reducer)
    {
        if(!_registratedActions.Add(reducer.ActionType)) 
            throw new InvalidOperationException("The ActionType is already Registrated");

        _actions
           .Via(_claseFlows.Flow<DispatchedAction<TState>>())
           .Via(RestartFlow.OnFailuresWithBackoff(
                () => reducer.Mutator.Recover(
                    ex =>
                    {
                        _onError(ex);
                        return Option<TState>.None;
                    }), 
                RestartSettings.Create(TimeSpan.MinValue, TimeSpan.MinValue, 0)))
           .RunWith(_stateSink, _materializer);
    }

    private void RegisterEffect(Effect<TState> effect)
    {
        var effectSource = RestartSource.OnFailuresWithBackoff(
            () => effect.CreateEffect(this).Recover(
                ex =>
                {
                    _onError(ex);

                    return Option<object?>.None;
                }), RestartSettings.Create(TimeSpan.MinValue, TimeSpan.MinValue, 0));

        _actionSink.RunWith(effectSource.Via(_claseFlows.Flow<object?>()), _materializer);
    }

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
}