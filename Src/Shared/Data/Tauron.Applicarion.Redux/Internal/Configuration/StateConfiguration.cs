using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions;
using Tauron.Operations;

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
    public IEffect<TState> CreateEffect(Func<IObservable<object>> run)
        => new SimpleEffect(run);

    public IEffect<TState> CreateEffect(Func<IObservable<TState>, IObservable<object>> run)
        => new SimpleStateEffect(run);

    public IEffect<TState> CreateEffect<TAction>(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run) where TAction : class
        => new ActionStateEffect<TAction>(run);

    private sealed class SimpleEffect : IEffect<TState>
    {
        private readonly Func<IObservable<object>> _run;

        public SimpleEffect(Func<IObservable<object>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(_run);
    }

    private sealed class SimpleStateEffect : IEffect<TState>
    {
        private readonly Func<IObservable<TState>, IObservable<object>> _run;

        public SimpleStateEffect(Func<IObservable<TState>, IObservable<object>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(store => _run(store.Select()));
    }

    private sealed class ActionStateEffect<TAction> : IEffect<TState> where TAction : class
    {
        private readonly Func<IObservable<(TAction Action, TState State)>, IObservable<object>> _run;

        public ActionStateEffect(Func<IObservable<(TAction Action, TState State)>, IObservable<object>> run)
            => _run = run;

        public Effect<TState> Build()
            => Create.Effect<TState>(
                s => _run(s.ObserveAction<TAction, (TAction, TState)>((action, state) => (action, state))));
    }
}

public sealed class RequesterFactory<TState> : IRequestFactory<TState> where TState : class, new()
{
    private readonly IErrorHandler _errorHandler;

    //private readonly List<Action<IReduxStore<TState>>> _registrar;
    private readonly List<Action<IRootStore, IReduxStore<TState>>> _registrar;
    private readonly IStateFactory _stateFactory;

    public RequesterFactory(List<Action<IRootStore, IReduxStore<TState>>> registrar, IErrorHandler errorHandler, IStateFactory stateFactory)
    {
        _registrar = registrar;
        _errorHandler = errorHandler;
        _stateFactory = stateFactory;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, null);

        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, ValueTask<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState> onFail) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, onFail);

        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, null);

        return this;
    }

    public IRequestFactory<TState> AddRequest<TAction>(Func<TAction, CancellationToken, Task<SimpleResult>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState> onFail) where TAction : class
    {
        AddRequestInternal(Adept(runRequest), onScess, onFail);

        return this;
    }

    public IRequestFactory<TState> OnTheFlyUpdate<TSource, TData>(Func<TState, TSource> sourceSelector, Func<CancellationToken, Func<CancellationToken, ValueTask<TSource>>, Task<TData>> fetcher, Func<TState, TData, TState> patcher)
    {
        _registrar.Add(
            (_, s) =>
            {
                DynamicUpdate.OnTheFlyUpdate(
                    s,
                    _stateFactory,
                    CreateSourceSelector(sourceSelector),
                    fetcher,
                    CreatePatacher(patcher),
                    _errorHandler.RequestError);
            });

        return this;
    }

    public IRequestFactory<TState> OnTheFlyUpdate<TData>(Func<CancellationToken, Task<TData>> fetcher, Func<TState, TData, TState> patcher)
    {
        _registrar.Add(
            (_, s) => DynamicUpdate.OnTheFlyUpdate(s, _stateFactory, fetcher, CreatePatacher(patcher), _errorHandler.RequestError));

        return this;
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


    private static Func<TState, object, TState> Adept(IErrorHandler errorHandler, Func<TState, object, TState>? updater)
        => updater ?? GetDefaultErrorHandler(errorHandler);

    private void AddRequestInternal<TAction>(Func<TAction, ValueTask<string?>> runRequest, Func<TState, TAction, TState> onScess, Func<TState, object, TState>? onFail)
        where TAction : class
    {
        Func<TAction, ValueTask<string?>> CreateRunner(Func<TAction, ValueTask<string?>> from)
            => async action => await from(action).ConfigureAwait(false);

        _registrar.Add(
            (s, _) => DynamicUpdate.AddRequest(s, CreateRunner(runRequest), onScess, Adept(_errorHandler, onFail)));
    }

    private static Selector<TState, TSource> CreateSourceSelector<TSource>(Func<TState, TSource> selector)
        => state => selector(state);

    private static Patcher<TData, TState> CreatePatacher<TData>(Func<TState, TData, TState> patcher)
        => (data, state) => patcher(state, data);
}

public sealed class StateConfiguration<TState> : IStateConfiguration<TState>
    where TState : class, new()
{
    private readonly List<Action<IRootStore, IReduxStore<TState>>> _config;
    private readonly IEffectFactory<TState> _effectFactory;
    private readonly List<IEffect<TState>> _effects = new();
    private readonly List<On<TState>> _reducer = new();

    private readonly IReducerFactory<TState> _reducerFactory;
    private readonly IRequestFactory<TState> _requestFactory;

    public StateConfiguration(IErrorHandler errorHandler, List<Action<IRootStore, IReduxStore<TState>>> config, IStateFactory stateFactory)
    {
        _config = config;

        config.Add(
            (_, s) =>
            {
                s.RegisterEffects(_effects.Select(e => e.Build()));
                s.RegisterReducers(_reducer);
            });

        _reducerFactory = new ReducerFactory<TState>();
        _effectFactory = new EffectFactory<TState>();
        _requestFactory = new RequesterFactory<TState>(config, errorHandler, stateFactory);
    }

    public IStateConfiguration<TState> ApplyReducer(Func<IReducerFactory<TState>, On<TState>> factory)
    {
        _reducer.Add(factory(_reducerFactory));

        return this;
    }

    public IStateConfiguration<TState> ApplyReducers(params Func<IReducerFactory<TState>, On<TState>>[] factorys)
    {
        _reducer.AddRange(factorys.Select(r => r(_reducerFactory)));

        return this;
    }

    public IStateConfiguration<TState> ApplyReducers(Func<IReducerFactory<TState>, IEnumerable<On<TState>>> factory)
    {
        _reducer.AddRange(factory(_reducerFactory));

        return this;
    }

    public IStateConfiguration<TState> ApplyEffect(Func<IEffectFactory<TState>, IEffect<TState>> factory)
    {
        _effects.Add(factory(_effectFactory));

        return this;
    }

    public IStateConfiguration<TState> ApplyEffects(params Func<IEffectFactory<TState>, IEffect<TState>>[] factorys)
    {
        _effects.AddRange(factorys.Select(f => f(_effectFactory)));

        return this;
    }

    public IStateConfiguration<TState> ApplyEffects(Func<IEffectFactory<TState>, IEnumerable<IEffect<TState>>> factory)
    {
        _effects.AddRange(factory(_effectFactory));

        return this;
    }

    public IStateConfiguration<TState> ApplyRequests(Action<IRequestFactory<TState>> factory)
    {
        factory(_requestFactory);

        return this;
    }

    public IStateConfiguration<TState> ApplyRequests(object factory1, params Action<IRequestFactory<TState>>[] factorys)
    {
        foreach (var factory in factorys)
            factory(_requestFactory);

        return this;
    }

    public IConfiguredState AndFinish(Action<IRootStore>? onCreate = null)
        => new ConfiguratedState<TState>(onCreate, _config);
}