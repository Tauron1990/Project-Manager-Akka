using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class ReducerFactory<TState> : IReducerFactory<TState>
    where TState : class, new()
{
    public On<TState> On<TAction>(Func<TState, TAction, TState> reducer) where TAction : class
        => Create.On(reducer);

    public On<TState> On<TAction>(Func<TState, TState> reducer) where TAction : class
        => Create.On<TAction, TState>(reducer);

    public IStateLens<TState, TFeatureState> CreateSubReducers<TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer) where TFeatureState : class, new()
        => Create.CreateSubReducers(featureSelector, stateReducer);
}

public sealed class EffectFactory<TState> : IEffectFactory<TState>
    where TState : class, new()
{
    private sealed class SimpleEffect : IEffect
    {
        private readonly Func<IObservable<object>> _run;

        public SimpleEffect(Func<IObservable<object>> run)
            => _run = run;

        public Effect<MultiState> Build()
            => Create.Effect<MultiState>(_run);
    }

    private sealed class SimpleStateEffect : IEffect
    {
        private readonly Func<IObservable<TState>, IObservable<object>> _run;
        private readonly Guid _state;

        public SimpleStateEffect(Func<IObservable<TState>, IObservable<object>> run, Guid state)
        {
            _run = run;
            _state = state;
        }

        public Effect<MultiState> Build()
            => Create.Effect<MultiState>(store => _run(store.Select(state => state.GetState<TState>(_state))));
    }
    
    private sealed class ActionStateEffect<TAction> : IEffect where TAction : class
    {
        private readonly Func<IObservable<(TAction Action, TState State)>, IObservable<object>> _run;
        private readonly Guid _state;

        public ActionStateEffect(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run, Guid state)
        {
            _run = run;
            _state = state;
        }
        
        public Effect<MultiState> Build()
            => Create.Effect<MultiState>(
                s => _run(s.ObserveAction<TAction, (TAction, TState)>((action, state) => (action, state.GetState<TState>(_state)))));
    }
    
    private readonly Guid _guid;
    
    public EffectFactory(Guid guid)
        => _guid = guid;

    public IEffect CreateEffect(Func<IObservable<object>> run)
        => new SimpleEffect(run);

    public IEffect CreateEffect(Func<IObservable<TState>, IObservable<object>> run)
        => new SimpleStateEffect(run, _guid);

    public IEffect CreateEffect<TAction>(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run) where TAction : class
        => new ActionStateEffect<TAction>(run, _guid);
}

public sealed class RequesterFactory<TState> : IRequestFactory<TState> where TState : class, new()
{
    private readonly List<Action<IReduxStore<MultiState>>> _registrar;
    private readonly Guid _stateId;
    private readonly IErrorHandler _errorHandler;
    private readonly IStateFactory _stateFactory;

    public RequesterFactory(List<Action<IReduxStore<MultiState>>> registrar, Guid stateId, IErrorHandler errorHandler, IStateFactory stateFactory)
    {
        _registrar = registrar;
        _stateId = stateId;
        _errorHandler = errorHandler;
        _stateFactory = stateFactory;
    }

    private static Func<TState, object, TState> GetDefaultErrorHandler(IErrorHandler errorHandler)
        => (state, o) =>
           {
               switch (o)
               {
                   case string text:
                       errorHandler.RequestError(text);
                       break;
                   case Exception exception:
                       errorHandler.RequestError(exception);
                       break;
               }
               return state;
           };

    private static Func<TAction, ValueTask<string?>> Adept<TAction>(Func<TAction, CancellationToken, ValueTask<string?>> request)
        => async a => await TimeoutToken.WithDefault(CancellationToken.None, async c => await request(a, c));

    private static Func<TAction, ValueTask<string?>> Adept<TAction>(Func<TAction, CancellationToken, Task<string?>> request)
        => async a => await TimeoutToken.WithDefault(CancellationToken.None, async c => await request(a, c));
    
    private static Func<MultiState, TAction, MultiState> Adept<TAction>(Guid id, Func<TState, TAction, TState> updater)
        => (s, a) => s.UpdateState(id, updater(s.GetState<TState>(id), a));

    private static Func<MultiState, object, MultiState> Adept(Guid id, IErrorHandler errorHandler, Func<TState, object, TState>? updater)
        => updater is null ? Adept(id, errorHandler, GetDefaultErrorHandler(errorHandler)) : (s, e) => s.UpdateState(id, updater(s.GetState<TState>(id), e));

    private void AddRequestInternal<TAction>(Func<TAction, ValueTask<string?>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState>? onFail) 
        where TAction : class
    {
        Func<TAction, ValueTask<string?>> CreateRunner(Func<TAction, ValueTask<string?>> from)
            => async action => await from(action);

        _registrar.Add(
            s => DynamicUpdate.AddRequest(s, CreateRunner(runRequest), Adept(_stateId, onScess), Adept(_stateId, _errorHandler, onFail)));
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<string?>> runRequest, Func<TState, TAction, TState> onScess) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, null);
        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<string?>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState> onFail) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, onFail);
        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<string?>> runRequest, Func<TState, TAction, TState> onScess) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, null);
        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<string?>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState> onFail) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, onFail);
        return this;
    }

    private static Selector<MultiState, TSource> CreateSourceSelector<TSource>(Guid id, Func<TState, TSource> selector)
        => state => selector(state.GetState<TState>(id));

    private static Patcher<TData, MultiState> CreatePatacher<TData>(Guid id, Func<TState, TData, TState> patcher)
        => (data, state) => state.UpdateState(id, patcher(state.GetState<TState>(id), data));

    public IRequestFactory<TState> OnTheFlyUpdate<TSource, TData>(Func<TState, TSource> sourceSelector, Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher, Func<TState, TData, TState> patcher)
    {
        Console.WriteLine("On the Fly Update");
        _registrar.Add(
            s =>
            {
                DynamicUpdate.OnTheFlyUpdate(s, _stateFactory,
                    CreateSourceSelector(_stateId, sourceSelector),
                    fetcher, CreatePatacher(_stateId, patcher));
            });
        return this;
    }

    public IRequestFactory<TState> OnTheFlyUpdate<TData>(Func<CancellationToken, Task<TData>> fetcher, Func<TState, TData, TState> patcher)
    {
        _registrar.Add(
            s =>
            {
                DynamicUpdate.OnTheFlyUpdate(s, _stateFactory, fetcher, CreatePatacher(_stateId, patcher));
            });
        
        return this;
    }
}

public sealed class StateConfiguration<TState> : IStateConfiguration<TState> 
    where TState : class, new() 
{
    private readonly Guid _stateId;

    private readonly List<On<MultiState>> _reducer = new();
    private readonly List<IEffect> _effects = new();

    private readonly IReducerFactory<TState> _reducerFactory;
    private readonly IEffectFactory<TState> _effectFactory;
    private readonly IRequestFactory<TState> _requestFactory;

    public StateConfiguration(
        Guid stateId, IErrorHandler errorHandler, List<Action<IReduxStore<MultiState>>> config, IStateFactory stateFactory)
    {
        _stateId = stateId;

        config.Add(s =>
                   {
                       s.RegisterEffects(_effects.Select(e => e.Build()));
                       s.RegisterReducers(_reducer);
                   });
        
        _reducerFactory = new ReducerFactory<TState>();
        _effectFactory = new EffectFactory<TState>(stateId);
        _requestFactory = new RequesterFactory<TState>(config, stateId, errorHandler, stateFactory);
    }
    
    
    private static On<MultiState> TransformOn(Guid id, On<TState> on)
    {
        MultiState Mutator(MultiState state, object action)
            => state.UpdateState(id, on.Mutator(state.GetState<TState>(id), action));

        return new On<MultiState>
               (
                   Mutator,
                   on.ActionType
               );
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

    public IConfiguredState AndFinish(Action<IRootStore>? onCreate = null)
        => new ConfiguratedState(typeof(TState), _stateId, onCreate);
}