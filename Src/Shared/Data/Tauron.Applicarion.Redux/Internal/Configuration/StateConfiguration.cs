using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

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