using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akka.Util;
using Autofac;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Application.Workshop.StateManagement.StatePooling;
using Tauron.Features;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public sealed record StateBuilderParameter(MutatingEngine Engine, IComponentContext? ComponentContext, IActionInvoker Invoker, StatePool StatePool, DispatcherPool DispatcherPool, 
        WorkspaceSuperviser Superviser);

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
        internal Type? State { get; private set; }

        public StateBuilder(Func<IExtendedDataSource<TData>> source) => _source = source;

        public IStateBuilder<TData> WithStateType<TState>()
            where TState : IState
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

        public override (StateContainer State, string Key) Materialize(StateBuilderParameter parameter)
        {
            var (engine, componentContext, invoker, statePool, dispatcherPool, superviser) = parameter;

            if (State == null)
                throw new InvalidOperationException("A State type or Instance Must be set");

            var cacheKey = $"{State.Name}--{Guid.NewGuid():N}";
            var dataSource = new MutationDataSource<TData>(cacheKey, _source());

            var pooledState = false;

            if (State.Implements<IPooledState>() && string.IsNullOrWhiteSpace(_dispatcherKey) && _dispatcher != null)
            {
                _dispatcherKey = State.AssemblyQualifiedName;
                pooledState = true;
            }

            ExtendedMutatingEngine<MutatingContext<TData>> dataEngine;
            if (string.IsNullOrWhiteSpace(_dispatcherKey) || _dispatcher == null)
            {
                dataEngine = _dispatcher == null
                    ? MutatingEngine.From(dataSource, engine)
                    : MutatingEngine.From(dataSource, engine.Superviser, _dispatcher().Configurate);
            }
            else
                dataEngine = MutatingEngine.From(dataSource, dispatcherPool.Get(_dispatcherKey, engine.Superviser, _dispatcher().Configurate));

            IStateInstance? Factory()
            {
                static string MakeName() => typeof(TData).Name + "-State";

                if (State.Implements<IFeature>())
                {
                    var factory = State.GetMethod("Create", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConventions.Standard,
                        Type.EmptyTypes, null);

                    if (factory == null)
                        return null;

                    return FastReflection.Shared.GetMethodInvoker(factory, () => Type.EmptyTypes)(null, null) is not IPreparedFeature feature 
                        ? null 
                        : new ActorRefInstance(superviser.CreateCustom(MakeName(), _ => Feature.Props(feature)), State);
                }

                if (State.Implements<ActorStateBase>())
                    return new ActorRefInstance(superviser.CreateCustom(MakeName(), _ => Props.Create(State)), State);

                IState? instance = null;

                if (componentContext != null) instance = componentContext.ResolveOptional(State, new TypedParameter(dataEngine.GetType(), dataEngine), new TypedParameter(typeof(IActionInvoker), invoker)) as IState;

                foreach (var constructorInfo in State.GetConstructors())
                {
                    var param = constructorInfo.GetParameters();

                    instance = param.Length switch
                    {
                        0 => FastReflection.Shared.FastCreateInstance(State) as IState,
                        1 => param[0].ParameterType.IsAssignableTo<IActionInvoker>()
                            ? FastReflection.Shared.GetCreator(constructorInfo)(new object?[]{ invoker }) as IState
                            : FastReflection.Shared.GetCreator(constructorInfo)(new object?[] { dataEngine }) as IState,
                        2 => FastReflection.Shared.GetCreator(constructorInfo)
                        (
                            param[0].ParameterType.IsAssignableTo<IActionInvoker>()
                            ? new object?[] { invoker, dataEngine }
                            : new object?[] { dataEngine, invoker }
                        ) as IState,
                        _ => instance
                    };

                    if(instance != null)
                        break;
                }

                if (instance == null)
                {
                    if (State.GetConstructors().Single().GetParameters().Length == 1)
                        instance = FastReflection.Shared.FastCreateInstance(State, dataEngine) as IState;
                    else
                        instance = FastReflection.Shared.FastCreateInstance(State, dataEngine, invoker) as IState;
                }

                return instance == null ? null : new PhysicalInstance(instance);
            }

            var targetState = pooledState ? statePool.Get(State, Factory) : Factory();

            if (targetState == null) throw new InvalidOperationException("Failed to Create State");

            targetState.InitState(dataEngine);
            targetState.ApplyQuery(dataSource);

            var container = new StateContainer<TData>(targetState, _reducers.Select(r => r()).ToImmutableList(), dataEngine, dataSource);

            return (container, _key ?? string.Empty);
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
    }
}