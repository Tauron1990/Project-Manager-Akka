using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using Tauron.Application.Workshop.Mutation;
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
        private string? _key;

        private Type? _state;

        public StateBuilder(Func<IExtendedDataSource<TData>> source) => _source = source;

        public IStateBuilder<TData> WithStateType<TState>()
            where TState : IState<TData>
        {
            _state = typeof(TState);
            return this;
        }

        public IStateBuilder<TData> WithStateType(Type type)
        {
            _state = type;
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
            if (_state == null)
                throw new InvalidOperationException("A State type or Instance Must be set");

            var cacheKey = $"{_state.Name}--{Guid.NewGuid():N}";
            var dataSource = new MutationDataSource<TData>(cacheKey, _source());

            var dataEngine = MutatingEngine.From(dataSource, engine);

            IState? targetState = null;

            if (componentContext != null)
                targetState = componentContext.ResolveOptional(_state, new TypedParameter(dataEngine.GetType(), dataEngine)) as IState;

            if (targetState == null)
            {
                if (_state.GetConstructors().Single().GetParameters().Length == 1)
                    targetState = FastReflection.Shared.FastCreateInstance(_state, dataEngine) as IState;
                else
                    targetState = FastReflection.Shared.FastCreateInstance(_state, dataEngine, invoker) as IState;
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
    }
}