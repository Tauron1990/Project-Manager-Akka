using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Tracing;
using System.Linq;
using Akka.Util;
using Autofac;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;
using Tauron.Application.Workshop.StateManagement.StatePooling;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public sealed record StateBuilderParameter(MutatingEngine engine, IComponentContext? ComponentContext, IActionInvoker Invoker, StatePool StatePool, DispatcherPool DispatcherPool);

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
            where TState : IState<TData>
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
            var (engine, componentContext, invoker, statePool, dispatcherPool) = parameter;

            if (State == null)
                throw new InvalidOperationException("A State type or Instance Must be set");

            var cacheKey = $"{State.Name}--{Guid.NewGuid():N}";
            var dataSource = new MutationDataSource<TData>(cacheKey, _source());

            bool pooledState = false;

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

            IState? Factory()
            {
                IState? instance = null;

                if (componentContext != null) instance = componentContext.ResolveOptional(State, new TypedParameter(dataEngine.GetType(), dataEngine), new TypedParameter(typeof(IActionInvoker), invoker)) as IState;

                foreach (var constructorInfo in State.GetConstructors())
                {
                    var param = constructorInfo.GetParameters();
                }

                if (instance == null)
                {
                    if (State.GetConstructors().Single().GetParameters().Length == 1)
                        instance = FastReflection.Shared.FastCreateInstance(State, dataEngine) as IState;
                    else
                        instance = FastReflection.Shared.FastCreateInstance(State, dataEngine, invoker) as IState;
                }

                return instance;
            }

            var targetState = pooledState ? statePool.Get(State, Factory) : Factory();

            switch (targetState)
            {
                case ICanQuery<TData> canQuery:
                    canQuery.DataSource(dataSource);
                    break;
                case null:
                    throw new InvalidOperationException("Failed to Create State");
            }

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
        }
    }
}