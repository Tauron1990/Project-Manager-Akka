using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
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
        var (engine, componentContext, invoker, statePool, dispatcherPool, instanceFactories) = parameter;

        if (State == null)
            throw new InvalidOperationException("A State type or Instance Must be set");

        //var cacheKey = $"{State.Name}--{Guid.NewGuid():N}";
        var dataSource = new MutationDataSource<TData>(_source());

        var pooledState = false;

        if (State.GetInterfaces().Contains(typeof(IPooledState)) && string.IsNullOrWhiteSpace(_dispatcherKey) && _dispatcher != null)
        {
            _dispatcherKey = State.AssemblyQualifiedName;
            pooledState = true;
        }

        var dataEngine = CreateDataEngine(dataSource, engine, dispatcherPool);

        IStateInstance? Factory() => CreateStateFactory(instanceFactories, componentContext, dataEngine, invoker);

        var targetState = pooledState ? statePool.Get(State, Factory) : Factory();

        if (targetState is null) throw new InvalidOperationException("Failed to Create State");

        targetState.InitState(dataEngine);
        targetState.ApplyQuery(dataSource);

        var container = new StateContainer<TData>(targetState, _reducers.Select(r => r()).ToImmutableList(), dataEngine, dataSource);

        return (container, _key ?? string.Empty);
    }

    private IStateInstance? CreateStateFactory(IStateInstanceFactory[] instanceFactories, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
    {
        if (State is null) return null;

        if (State.Implements<IFeature>())
        {
            return CreateFeatureState(superviser);
        }

        if (State.Implements<ActorStateBase>())
            return new ActorRefInstance(superviser.CreateCustom(MakeName(), _ => Props.Create(State)), State);

        object? instance = null;

        if (serviceProvider != null)
            instance = ActivatorUtilities.CreateInstance(serviceProvider, State, dataEngine, invoker);

        if (instance is not null) return new PhysicalInstance(instance);

        instance = CreateStateFromConstructor(dataEngine, invoker, instance);

        if (instance is null)
        {
            if (State.GetConstructors().Single().GetParameters().Length == 1)
                instance = FastReflection.Shared.FastCreateInstance(State, dataEngine);
            else
                instance = FastReflection.Shared.FastCreateInstance(State, dataEngine, invoker);
        }

        return instance is null ? null : new PhysicalInstance(instance);
    }

    private object? CreateStateFromConstructor(ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker, object? instance)
    {
        if (State is null) return null;
        
        foreach (var constructorInfo in State.GetConstructors())
        {
            var param = constructorInfo.GetParameters();

            instance = param.Length switch
            {
                0 => FastReflection.Shared.FastCreateInstance(State),
                1 => param[0].ParameterType.IsAssignableTo(typeof(IActionInvoker))
                    ? FastReflection.Shared.GetCreator(constructorInfo)(new object?[] { invoker })
                    : FastReflection.Shared.GetCreator(constructorInfo)(new object?[] { dataEngine }),
                2 => FastReflection.Shared.GetCreator(constructorInfo)
                (
                    param[0].ParameterType.IsAssignableTo(typeof(IActionInvoker))
                        ? new object?[] { invoker, dataEngine }
                        : new object?[] { dataEngine, invoker }
                ),
                _ => instance
            };

            if (instance != null)
                break;
        }

        return instance;
    }

    private IStateInstance? CreateFeatureState(WorkspaceSuperviser superviser)
    {
        if (State is null) return null;
        
        var factory = State.GetMethod(
            "Create",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
            null,
            CallingConventions.Standard,
            Type.EmptyTypes,
            null);

        if (factory == null)
            return null;

        return FastReflection.Shared.GetMethodInvoker(factory, () => Type.EmptyTypes)(null, null) is not IPreparedFeature feature
            ? null
            : new ActorRefInstance(superviser.CreateCustom(MakeName(), _ => Feature.Props(feature)), State);
    }

    private static string MakeName()
        => typeof(TData).Name + "-State";

    private ExtendedMutatingEngine<MutatingContext<TData>> CreateDataEngine(MutationDataSource<TData> dataSource, MutatingEngine engine, DispatcherPool dispatcherPool)
    {
        ExtendedMutatingEngine<MutatingContext<TData>> dataEngine;
        if (string.IsNullOrWhiteSpace(_dispatcherKey) || _dispatcher == null)
            dataEngine = _dispatcher == null
                ? MutatingEngine.From(dataSource, engine)
                : MutatingEngine.From(dataSource, engine.Superviser, _dispatcher().Configurate);
        else
            dataEngine = MutatingEngine.From(dataSource, dispatcherPool.Get(_dispatcherKey, engine.Superviser, _dispatcher().Configurate));

        return dataEngine;
    }
}