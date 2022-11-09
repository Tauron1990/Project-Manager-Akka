using System.Collections.Immutable;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Application.Workshop.StateManagement.StatePooling;

namespace Tauron.Application.Workshop.StateManagement.Builder;

public sealed record StateBuilderParameter(
    MutatingEngine Engine, IServiceProvider? ServiceProvider, IActionInvoker Invoker, StatePool StatePool, DispatcherPool DispatcherPool,
    IStateInstanceFactory[] InstanceFactoys);

public abstract class StateBuilderBase
{
    public abstract (StateContainer State, string Key) Materialize(StateBuilderParameter parameter);
}

public sealed class StateBuilder<TData> : StateBuilderBase, IStateBuilder<TData>
    where TData : class, IStateEntity
{
    private readonly List<Func<IReducer<TData>>> _reducers = new();
    private readonly Func<IExtendedDataSource<TData>> _source;

    private Func<IStateDispatcherConfigurator>? _dispatcher;
    private string? _dispatcherKey;
    private string? _key;

    public StateBuilder(Func<IExtendedDataSource<TData>> source) => _source = source;
    internal Type? State { get; private set; }

    public IStateBuilder<TData> WithStateType<TState>()
    {
        State = typeof(TState);

        return this;
    }

    public IStateBuilder<TData> WithStateType(Type type)
    {
        State = type;

        return this;
    }

    public IStateBuilder<TData> WithReducer(Func<IReducer<TData>> reducer)
    {
        _reducers.Add(reducer);

        return this;
    }

    public IStateBuilder<TData> WithKey(string key)
    {
        _key = key;

        return this;
    }

    public IStateBuilder<TData> WithDispatcher(Func<IStateDispatcherConfigurator>? factory)
    {
        _dispatcher = factory;
        _dispatcherKey = null;

        return this;
    }

    public IStateBuilder<TData> WithDispatcher(string name, Func<IStateDispatcherConfigurator>? factory)
    {
        _dispatcherKey = name;
        _dispatcher = factory;

        return this;
    }

    public override (StateContainer State, string Key) Materialize(StateBuilderParameter parameter)
    {
        (MutatingEngine engine, IServiceProvider? serviceProvider, IActionInvoker invoker, StatePool statePool, DispatcherPool dispatcherPool, var instanceFactories) = parameter;

        if(State == null)
            throw new InvalidOperationException("A State type or Instance Must be set");

        //var cacheKey = $"{State.Name}--{Guid.NewGuid():N}";
        var dataSource = new MutationDataSource<TData>(_source());

        var pooledState = false;

        if(State.GetInterfaces().Contains(typeof(IPooledState)) && string.IsNullOrWhiteSpace(_dispatcherKey) && _dispatcher != null)
        {
            _dispatcherKey = State.AssemblyQualifiedName;
            pooledState = true;
        }

        var dataEngine = CreateDataEngine(dataSource, engine, dispatcherPool);

        IStateInstance? Factory() => CreateStateFactory(instanceFactories, serviceProvider, dataEngine, invoker);

        IStateInstance? targetState = pooledState ? statePool.Get(State, Factory) : Factory();

        if(targetState is null) throw new InvalidOperationException("Failed to Create State");

        targetState.InitState(dataEngine);
        targetState.ApplyQuery(dataSource);

        var container = new StateContainer<TData>(targetState, _reducers.Select(r => r()).ToImmutableList(), dataEngine, dataSource);

        return (container, _key ?? string.Empty);
    }

    private IStateInstance? CreateStateFactory(IEnumerable<IStateInstanceFactory> instanceFactories, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
    {
        if(State is null) return null;

        return
        (
            from factory in instanceFactories
            orderby factory.Order
            where factory.CanCreate(State)
            select factory.Create(State, serviceProvider, dataEngine, invoker)
            into inst
            where inst is not null
            select inst
        ).First();

    }

    private ExtendedMutatingEngine<MutatingContext<TData>> CreateDataEngine(MutationDataSource<TData> dataSource, MutatingEngine engine, DispatcherPool dispatcherPool)
    {
        ExtendedMutatingEngine<MutatingContext<TData>> dataEngine;
        if(string.IsNullOrWhiteSpace(_dispatcherKey) || _dispatcher == null)
            dataEngine = _dispatcher == null
                ? MutatingEngine.From(dataSource, engine)
                : MutatingEngine.From(dataSource, _dispatcher().Configurate(engine.DriverFactory));
        else
            dataEngine = MutatingEngine.From(dataSource, dispatcherPool.Get(_dispatcherKey, _dispatcher().Configurate(engine.DriverFactory)));

        return dataEngine;
    }
}