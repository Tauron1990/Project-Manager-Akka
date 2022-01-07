using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ReduxSimple;
using Stl.Fusion;
using Tauron;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Applicarion.Redux.Extensions.Internal;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class SourceConfiguration<TState> : ISourceConfiguration<TState> 
    where TState : class, new()
{
    private sealed record SetNewState(Func<TState, TState> StateMutator);
    
    private sealed class StateFactory<TToPatch>
    {
        private readonly Func<CancellationToken, Task<TToPatch>> _runner;
        private readonly IStateFactory _builder;
        private readonly CompositeDisposable _diposer;

        private IDisposable? _currentState;
        
        public StateFactory(Func<CancellationToken, Task<TToPatch>> runner, IStateFactory builder, CompositeDisposable diposer)
        {
            _runner = runner;
            _builder = builder;
            _diposer = diposer;
        }

        public IComputedState<TToPatch> Create()
        {
            if (_currentState is not null)
                _diposer.Remove(_currentState);

            var state = _builder.NewComputed<TToPatch>(async (_, token) => await _runner(token));
            _diposer.Add(state);
            _currentState = state;

            return state;
        }
    }
    
    private sealed class FetcherEffect<TToPatch> : IEffect
    {
        private readonly Func<IComputedState<TToPatch>> _computedState;
        private readonly Func<TState, TToPatch, TState> _patcher;
        private readonly CompositeDisposable _disposer;

        public FetcherEffect(Func<IComputedState<TToPatch>> computedState, Func<TState, TToPatch, TState> patcher, CompositeDisposable disposer)
        {
            _computedState = computedState;
            _patcher = patcher;
            _disposer = disposer;
        }
        
        public Effect<MultiState> Build()
            => Effects.CreateEffect<MultiState>(BuildObserable(_patcher), true);

        private Func<IObservable<object>> BuildObserable(Func<TState, TToPatch, TState> patcher)
            => () => from toPatch in BuildFetcher()
               select new SetNewState(s => patcher(s, toPatch));

        private IObservable<TToPatch> BuildFetcher()
        {
            var state = _computedState();
            _disposer.Add(state);

            return state.ToObservable(true);
        }
    }
    
    private sealed class FromDatatabeFecther : IEffect
    {
        private readonly Task<TState?> _provider;
        private readonly IEffect _serverConnect;

        public FromDatatabeFecther(Task<TState?> provider, IEffect serverConnect)
        {
            _provider = provider;
            _serverConnect = serverConnect;
        }

        public Effect<MultiState> Build()
            => Effects.CreateEffect<MultiState>(
                s => (
                    from state in _provider.ToObservable()
                    where state is not null
                    select new SetNewState(_ => state)
                ).Finally(() => s.RegisterEffects(_serverConnect.Build())),
                true);
    }
    
    private sealed class DatabaseUpdater : IEffect
    {
        private readonly StateDb _db;
        private readonly Guid _id;

        public DatabaseUpdater(StateDb db, Guid id)
        {
            _db = db;
            _id = id;
        }

        public Effect<MultiState> Build()
            => Effects.CreateEffect<MultiState>(
                s => 
                    s.Select(applicationState => applicationState.GetState<TState>(_id))
                   .ToUnit(state => _db.Set(state))
                   .Select(_ => (object)_db),
                false);
    }
    
    private sealed class InitialProvider : IEffect
    {
        private readonly TState _initial;

        public InitialProvider(TState initial)
        {
            _initial = initial;
        }
        
        public Effect<MultiState> Build()
            => Effects.CreateEffect<MultiState>(_ => Observable.Return(_initial).Select(s => new SetNewState(_ => s)),  true);
    }
    
    private readonly IStateFactory _stateFactory;
    private readonly IEventAggregator _aggregator;
    private readonly Guid _guid;
    private readonly CompositeDisposable _disposer;
    private readonly StateDb _stateDb;

    public SourceConfiguration(IStateFactory stateFactory, IEventAggregator aggregator, Guid guid, CompositeDisposable disposer, StateDb stateDb)
    {
        _stateFactory = stateFactory;
        _aggregator = aggregator;
        _guid = guid;
        _disposer = disposer;
        _stateDb = stateDb;
    }

    private static On<MultiState> Setter(Guid id)
        => Reducers.On<SetNewState, MultiState>(
            (state, newState)
                => state.UpdateState(id, newState.StateMutator(state.GetState<TState>(id))));

    private FetcherEffect<TToPatch> CreateState<TToPatch>(
        Func<CancellationToken, Task<TToPatch>> fetcher, 
        Func<TState, TToPatch, TState> patcher, 
        out Action<List<On<MultiState>>, List<IEffect>> config)
    {
        config = (ons, _) => ons.Add(Setter(_guid));
        
        return new(new StateFactory<TToPatch>(fetcher, _stateFactory, _disposer).Create, patcher, _disposer);
    }

    private FromDatatabeFecther CreateDatabase<TToPatch>(
        Func<CancellationToken, Task<TToPatch>> fetcher, 
        Func<TState, TToPatch, TState> patcher,
        out Action<List<On<MultiState>>, List<IEffect>> config)
    {
        var effect = new FromDatatabeFecther(_stateDb.Get<TState>(), CreateState(fetcher, patcher, out var subConfig));
        config = (ons, list) =>
                 {
                     subConfig(ons, list);
                 };

        return effect;
    }

    private Action<List<On<MultiState>>, List<IEffect>> CreateConfigurationForDatabase<TToPatch>(       
        Func<CancellationToken, Task<TToPatch>> fetcher, 
        Func<TState, TToPatch, TState> patcher)
        => (ons, list) =>
           {
               list.Add(CreateDatabase(fetcher, patcher, out var config));
               list.Add(new DatabaseUpdater(_stateDb, _guid));
               config(ons, list);
           };

    private Action<List<On<MultiState>>, List<IEffect>> CreateConfiguratorForServerOnly<TToPatch>(
        Func<CancellationToken, Task<TToPatch>> fetcher,
        Func<TState, TToPatch, TState> patcher)
        => (ons, list) =>
           {
               list.Add(CreateState(fetcher, patcher, out var config));
               config(ons, list);
           };

    private IStateConfiguration<TState> CreateConfiguration(Action<List<On<MultiState>>, List<IEffect>> config)
        => new StateConfiguration<TState>(_guid, _aggregator, config, _disposer, _stateFactory);

    public IStateConfiguration<TState> FromInitial(TState? initial = default)
        => CreateConfiguration(
            (ons, list) =>
            {
                if (initial is null) return;

                ons.Add(Setter(_guid));
                list.Add(new InitialProvider(initial));
            });

    public IStateConfiguration<TState> FromServer(Func<CancellationToken, Task<TState>> fetcher)
        => CreateConfiguration(CreateConfiguratorForServerOnly(fetcher, (_, newState) => newState));

    public IStateConfiguration<TState> FromServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
        => CreateConfiguration(CreateConfiguratorForServerOnly(fetcher, patcher));

    public IStateConfiguration<TState> FromCacheAndServer(Func<CancellationToken, Task<TState>> fetcher)
        => CreateConfiguration(CreateConfigurationForDatabase(fetcher, (_, newState) => newState));

    public IStateConfiguration<TState> FromCacheAndServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
        => CreateConfiguration(CreateConfigurationForDatabase(fetcher, patcher));
}