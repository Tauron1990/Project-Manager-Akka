using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReduxSimple;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class SelectorFactory<TState> : ISelectorFactory<TState>
{
    public ISelectorWithoutProps<TState, TFinalResult> CreateSelector<TSelectorResult1, TFinalResult>(ISelectorWithoutProps<TState, TSelectorResult1> selector1, Func<TSelectorResult1, TFinalResult> projectorFunction)
        => new MemoizedSelector<TState,TSelectorResult1,TFinalResult>(selector1, projectorFunction);

    public ISelectorWithoutProps<TState, TFinalResult> CreateSelector<TSelectorResult1, TSelectorResult2, TFinalResult>(ISelectorWithoutProps<TState, TSelectorResult1> selector1, ISelectorWithoutProps<TState, TSelectorResult2> selector2, Func<TSelectorResult1, TSelectorResult2, TFinalResult> projectorFunction)
        => new MemoizedSelector<TState, TSelectorResult1, TSelectorResult2, TFinalResult>(selector1, selector2, projectorFunction);

    public ISelectorWithProps<TState, TProps, TFinalResult> CreateSelector<TProps, TSelectorResult1, TFinalResult>(ISelector<TState, TSelectorResult1> selector1, Func<TSelectorResult1, TProps, TFinalResult> projectorFunction)
        => new MemoizedSelectorWithProps<TState,TProps,TSelectorResult1,TFinalResult>(selector1, projectorFunction);

    public ISelectorWithProps<TState, TProps, TFinalResult> CreateSelector<TProps, TSelectorResult1, TSelectorResult2, TFinalResult>(ISelector<TState, TSelectorResult1> selector1, ISelector<TState, TSelectorResult2> selector2, Func<TSelectorResult1, TSelectorResult2, TProps, TFinalResult> projectorFunction)
        => new MemoizedSelectorWithProps<TState,TProps,TSelectorResult1,TSelectorResult2,TFinalResult>(selector1, selector2, projectorFunction);

    public ISelectorWithoutProps<TState, TResult> CreateSelector<TResult>(Func<TState, TResult> selector)
        => new SimpleSelector<TState, TResult>(selector);
}

public sealed class ReducerFactory<TState> : IReducerFactory<TState>
    where TState : class, new()
{
    public IEnumerable<On<TState>> CombineReducers(params IEnumerable<On<TState>>[] reducersList)
        => Reducers.CombineReducers(reducersList);

    public On<TState> On<TAction>(Func<TState, TAction, TState> reducer) where TAction : class
        => Reducers.On(reducer);

    public On<TState> On<TAction>(Func<TState, TState> reducer) where TAction : class
        => Reducers.On<TAction, TState>(reducer);

    public On<TState> On<TAction1, TAction2>(Func<TState, TState> reducer) where TAction1 : class where TAction2 : class
        => Reducers.On<TAction1, TAction2, TState>(reducer);

    public On<TState> On<TAction1, TAction2, TAction3>(Func<TState, TState> reducer) where TAction1 : class where TAction2 : class where TAction3 : class
        => Reducers.On<TAction1, TAction2, TAction3, TState>(reducer);

    public On<TState> On<TAction1, TAction2, TAction3, TAction4>(Func<TState, TState> reducer) where TAction1 : class where TAction2 : class where TAction3 : class where TAction4 : class
        => Reducers.On<TAction1, TAction2, TAction3, TAction4, TState>(reducer);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector) where TFeatureState : class, new()
        => Reducers.CreateSubReducers(featureSelector);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TAction, TFeatureState>(Func<TState, TAction, TFeatureState> featureSelector) where TFeatureState : class, new()
        => Reducers.CreateSubReducers(featureSelector);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(ISelectorWithoutProps<TState, TFeatureState> featureSelector) where TFeatureState : class, new()
        => Reducers.CreateSubReducers(featureSelector);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer) where TFeatureState : class, new()
        => Reducers.CreateSubReducers(featureSelector, stateReducer);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TAction, TFeatureState>(Func<TState, TAction, TFeatureState> featureSelector, Func<TState, TAction, TFeatureState, TState> stateReducer) where TFeatureState : class, new()
        => Reducers.CreateSubReducers(featureSelector, stateReducer);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(ISelectorWithoutProps<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer) where TFeatureState : class, new()
        => Reducers.CreateSubReducers(featureSelector, stateReducer);
}

public sealed class EffectFactory<TState> : IEffectFactory<TState>
    where TState : class, new()
{
    private sealed class SimpleEffect : IEffect
    {
        private readonly Func<IObservable<object>> _run;
        private readonly bool _dispatch;

        public SimpleEffect(Func<IObservable<object>> run, bool dispatch)
        {
            _run = run;
            _dispatch = dispatch;
        }
        
        public Effect<ApplicationState> Build()
            => Effects.CreateEffect<ApplicationState>(_run, _dispatch);
    }

    private sealed class SimpleStateEffect : IEffect
    {
        private readonly Func<IObservable<TState>, IObservable<object>> _run;
        private readonly Guid _state;
        private readonly bool _dispatch;

        public SimpleStateEffect(Func<IObservable<TState>, IObservable<object>> run, Guid state, bool dispatch)
        {
            _run = run;
            _state = state;
            _dispatch = dispatch;
        }

        public Effect<ApplicationState> Build()
            => Effects.CreateEffect<ApplicationState>(store => _run(store.Select(state => state.GetState<TState>(_state))), _dispatch);
    }
    
    private sealed class ActionStateEffect<TAction> : IEffect
    {
        private readonly Func<IObservable<(TAction Action, TState State)>, IObservable<object>> _run;
        private readonly Guid _state;
        private readonly bool _dispatch;

        public ActionStateEffect(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run, Guid state, bool dispatch)
        {
            _run = run;
            _state = state;
            _dispatch = dispatch;
        }
        
        public Effect<ApplicationState> Build()
            => Effects.CreateEffect<ApplicationState>(
                s => _run(s.ObserveAction<TAction, (TAction, TState)>((action, state) => (action, state.GetState<TState>(_state)))),
                _dispatch);
    }
    
    private readonly Guid _guid;
    
    public EffectFactory(Guid guid)
        => _guid = guid;

    public IEffect CreateEffect(Func<IObservable<object>> run, bool dispatch = true)
        => new SimpleEffect(run, dispatch);

    public IEffect CreateEffect(Func<IObservable<TState>, IObservable<object>> run, bool dispatch = true)
        => new SimpleStateEffect(run, _guid, dispatch);

    public IEffect CreateEffect<TAction>(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run, bool dispatch = true)
        => new ActionStateEffect<TAction>(run, _guid, dispatch);
}

public sealed class RequesterFactory<TState> : IRequestFactory<TState> 
    where TState : class, new()
{
    private sealed record StartRequest<TAction>(TAction Action, Func<TAction, Task<string>> RequestRunner,  Func<TState, TAction, TState> OnSucess, Func<TState, object, TState> OnFail);

    // ReSharper disable once UnusedTypeParameter
    private sealed record FinishRequest<TAction>(Func<TState, TState> Apply);
    
    private sealed class RequestRegister<TAction> : IEffect
    {
        private readonly Func<TAction, Task<string>> _runRequest;
        private readonly Func<TState, TAction, TState> _onSucess;
        private readonly Func<TState, object, TState> _onFail;

        public RequestRegister(Func<TAction, Task<string>> runRequest,  Func<TState, TAction, TState> onSucess, Func<TState, object, TState> onFail)
        {
            _runRequest = runRequest;
            _onSucess = onSucess;
            _onFail = onFail;
        }

        private IObservable<object> Process(ReduxStore<ApplicationState> store)
            => from input in store.ObserveAction<TAction, (TAction Action, ApplicationState State)>((action, applicationState) => (action, applicationState))
               select new StartRequest<TAction>(input.Action, _runRequest, _onSucess, _onFail);

        public Effect<ApplicationState> Build()
            => Effects.CreateEffect<ApplicationState>(Process, true);
    }
    
    private sealed class RequestProcessor<TAction> : IEffect
    {
        private IObservable<object> Process(ReduxStore<ApplicationState> store)
            => store.ObserveAction<StartRequest<TAction>>()
               .CatchSafe(
                    d => from data in Observable.Return(d)
                         from result in d.RequestRunner(data.Action)
                         select string.IsNullOrWhiteSpace(result)
                             ? new FinishRequest<TAction>(s => data.OnSucess(s, data.Action))
                             : new FinishRequest<TAction>(s => data.OnFail(s, result)),
                    (d, e) => Observable.Return(new FinishRequest<TAction>(s => d.OnFail(s, e))));

        public Effect<ApplicationState> Build()
            => Effects.CreateEffect<ApplicationState>(Process, true);
    }

    private sealed record UpdateState(Func<TState, TState> Updater);

    private sealed class DynamicUpdater<TSource, TData> : IEffect
    {
        private readonly IMutableState<TSource> _state;
        private readonly IStateFactory _stateFactory;
        private readonly Func<TState, TSource> _sourceSelector;
        private readonly Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> _fetcher;
        private readonly Func<TState, TData, TState> _patcher;
        private readonly CompositeDisposable _disposer;
        private readonly Guid _id;

        public DynamicUpdater( 
            IStateFactory stateFactory,
            Func<TState, TSource> sourceSelector,
            Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher, 
            Func<TState, TData, TState> patcher,
            CompositeDisposable disposer,
            Guid id)
        {
            _state = stateFactory.NewMutable<TSource>();

            _stateFactory = stateFactory;
            _sourceSelector = sourceSelector;
            _fetcher = fetcher;
            _patcher = patcher;
            _disposer = disposer;
            _id = id;
        }

        public Effect<ApplicationState> Build()
            => Effects.CreateEffect<ApplicationState>(
                store =>
                {
                    var source = store.Select(state => state.GetState<TState>(_id)).Select(_sourceSelector).DistinctUntilChanged();
                    _disposer.Add(source.Subscribe(s => _state.Set(s)));
                    var fetcherState = _stateFactory.NewComputed<TData>(async (_, c) => await _fetcher(c, _state.Use))
                       .DisposeWith(_disposer);

                    return from newData in fetcherState.ToObservable(true)
                           select new UpdateState(s => _patcher(s, newData));
                }, true);
    }
    
    private readonly HashSet<Type> _actions = new();

    private readonly IEventAggregator _aggregator;
    private readonly Guid _stateId;
    private readonly List<On<ApplicationState>> _reducers;
    private readonly List<IEffect> _effects;
    private readonly IStateFactory _stateFactory;
    private readonly CompositeDisposable _compositeDisposable;

    public RequesterFactory(
        IEventAggregator aggregator, 
        Guid stateId, 
        List<On<ApplicationState>> reducers, 
        List<IEffect> effects, 
        IStateFactory stateFactory, 
        CompositeDisposable compositeDisposable)
    {
        _aggregator = aggregator;
        _stateId = stateId;
        _reducers = reducers;
        _effects = effects;
        _stateFactory = stateFactory;
        _compositeDisposable = compositeDisposable;

        AddUpdater(reducers, stateId);
    }

    private void AddUpdater(IList<On<ApplicationState>> reducers, Guid stateId)
        => reducers.Add(
            Reducers.On<UpdateState, ApplicationState>(
                (state, action) => state.UpdateState(stateId, action.Updater(state.GetState<TState>(stateId)))));
    
    private void RegisterEffectForAction<TAction>(
        Func<TAction, Task<string>> runRequest,  
        Func<TState, TAction, TState> onSucess, 
        Func<TState, object, TState> onFail) 
        where TAction : class
    {
        if (_actions.Add(typeof(TAction)))
        {
            _effects.AddRange(
                new IEffect[]
                {
                    new RequestRegister<TAction>(runRequest, onSucess, onFail),
                    new RequestProcessor<TAction>()
                });
            _reducers.AddRange(CreateRequestFinisher<TAction>(_stateId));
        }
        else
            throw new InvalidOperationException("Action Already Registrated");
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, Task<string>> runRequest, Func<TState, TAction, TState> onScess) 
        where TAction : class
        => AddRequest(runRequest, onScess, GetDefaultErrorHandler(_aggregator));

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, Task<string>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState> onFail) 
        where TAction : class
    {
        RegisterEffectForAction(runRequest, onScess, onFail);

        return this;
    }

    public IRequestFactory<TState> OnTheFlyUpdate<TSource, TData>(
        Func<TState, TSource> sourceSelector,
        Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher, 
        Func<TState, TData, TState> patcher)
    {
        _effects.Add(new DynamicUpdater<TSource,TData>(_stateFactory, sourceSelector, fetcher, patcher, _compositeDisposable, _stateId));

        return this;
    }

    private static IEnumerable<On<ApplicationState>> CreateRequestFinisher<TAction>(Guid stateId)
        => Reducers.CreateSubReducers<ApplicationState, TState>(
                state => state.GetState<TState>(stateId),
                (state, subState) => state.UpdateState(stateId, subState))
           .On<FinishRequest<TAction>>((state, action) => action.Apply(state))
           .ToList();

    private static Func<TState, object, TState> GetDefaultErrorHandler(IEventAggregator aggregator)
        => (state, o) =>
           {
               switch (o)
               {
                   case string text:
                       aggregator.PublishError(text);
                       break;
                   case Exception exception:
                       aggregator.PublishError(exception);
                       break;
               }
               return state;
           };
}
    
public sealed class StateConfiguration<TState> : IStateConfiguration<TState> 
    where TState : class, new() 
{
    private readonly Guid _stateId;

    private readonly List<On<ApplicationState>> _reducer = new();
    private readonly List<IEffect> _effects = new();

    private readonly IReducerFactory<TState> _reducerFactory;
    private readonly IEffectFactory<TState> _effectFactory;
    private readonly IRequestFactory<TState> _requestFactory;

    public StateConfiguration(
        Guid stateId, IEventAggregator eventAggregator, Action<List<On<ApplicationState>>, List<IEffect>> config, 
        CompositeDisposable diposer, IStateFactory stateFactory)
    {
        _stateId = stateId;

        config(_reducer, _effects);
        
        SelectorFactory = new SelectorFactory<TState>();
        _reducerFactory = new ReducerFactory<TState>();
        _effectFactory = new EffectFactory<TState>(stateId);
        _requestFactory = new RequesterFactory<TState>(eventAggregator, stateId, _reducer, _effects, stateFactory, diposer);
    }
    
    public ISelectorFactory<TState> SelectorFactory { get; }
    
    private static On<ApplicationState> TransformOn(Guid id, On<TState> on)
    {
        ApplicationState Run(ApplicationState state, object action)
            => on.Reduce == null ? state : state.UpdateState(id, on.Reduce(state.GetState<TState>(id), action));

        return new On<ApplicationState>
               {
                   Reduce = Run,
                   Types = on.Types
               };
    }

    public IStateConfiguration<TState> ApplyReducer(Func<IReducerFactory<TState>, On<TState>> factory)
    {
        _reducer.Add(TransformOn(_stateId, factory(_reducerFactory)));
        return this;
    }

    public IStateConfiguration<TState> ApplyReducers(params Func<IReducerFactory<TState>, On<TState>>[] factorys)
    {
        _reducer.AddRange(factorys.Select(r => TransformOn(_stateId, r(_reducerFactory))));
        return this;
    }

    public IStateConfiguration<TState> ApplyReducers(Func<IReducerFactory<TState>, IEnumerable<On<TState>>> factory)
    {
        _reducer.AddRange(factory(_reducerFactory).Select(r => TransformOn(_stateId, r)));
        return this;
    }

    public IStateConfiguration<TState> ApplyEffect(Func<IEffectFactory<TState>, IEffect> factory)
    {
        _effects.Add(factory(_effectFactory));
        return this;
    }

    public IStateConfiguration<TState> ApplyEffects(params Func<IEffectFactory<TState>, IEffect>[] factorys)
    {
        _effects.AddRange(factorys.Select(f => f(_effectFactory)));
        return this;
    }

    public IStateConfiguration<TState> ApplyEffects(Func<IEffectFactory<TState>, IEnumerable<IEffect>> factory)
    {
        _effects.AddRange(factory(_effectFactory));
        return this;
    }

    public IStateConfiguration<TState> ApplyRequests(Action<IRequestFactory<TState>> factory)
    {
        factory(_requestFactory);
        return this;
    }

    public IStateConfiguration<TState> ApplyRequests(params Action<IRequestFactory<TState>>[] factorys)
    {
        foreach (var factory in factorys)
        {
            factory(_requestFactory);
        }

        return this;
    }

    public IConfiguredState AndFinish()
        => new ConfiguratedState(_reducer, _effects, typeof(TState), _stateId);
}