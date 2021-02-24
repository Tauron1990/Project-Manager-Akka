using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Dispatcher;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    public abstract class StateBuilderBase
    {
        public abstract (StateContainer State, string Key) Materialize(MutatingEngine engine, IComponentContext? componentContext, IActionInvoker invoker);
    }

    public sealed class StateBuilder<TData> : StateBuilderBase, IStateBuilder<TData>
        where TData : class, IStateEntity
    {
        private readonly List<Func<IReducer<TData>>> _reducers = new();
        private readonly Func<IExtendedDataSource<TData>> _source;

        private Func<IStateDispatcherConfigurator>? _dispatcher;
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

        public override (StateContainer State, string Key) Materialize(MutatingEngine engine, IComponentContext? componentContext, IActionInvoker invoker)
        {
            if (State == null)
                throw new InvalidOperationException("A State type or Instance Must be set");

            var cacheKey = $"{State.Name}--{Guid.NewGuid():N}";
            var dataSource = new MutationDataSource<TData>(cacheKey, _source());

            ExtendedMutatingEngine<MutatingContext<TData>> dataEngine;
            if (_dispatcher == null)
                dataEngine = MutatingEngine.From(dataSource, engine);
            else
                dataEngine = MutatingEngine.From(dataSource, engine.Superviser, _dispatcher().Configurate);

            IState? targetState = null;

            if (componentContext != null)
                targetState = componentContext.ResolveOptional(State, new TypedParameter(dataEngine.GetType(), dataEngine)) as IState;

            if (targetState == null)
            {
                if (State.GetConstructors().Single().GetParameters().Length == 1)
                    targetState = FastReflection.Shared.FastCreateInstance(State, dataEngine) as IState;
                else
                    targetState = FastReflection.Shared.FastCreateInstance(State, dataEngine, invoker) as IState;
            }

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

        public IStateBuilder<TData> WithDispatcher(Func<IStateDispatcherConfigurator> factory)
        {
            _dispatcher = factory;
            return this;
        }
    }
}