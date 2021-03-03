using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public sealed class ManagerBuilder : IDispatcherConfigurable<ManagerBuilder>
    {
        private readonly List<Func<IEffect>> _effects = new();
        private readonly List<Func<IMiddleware>> _middlewares = new();
        private readonly List<StateBuilderBase> _states = new();

        private Func<IStateDispatcherConfigurator> _dispatcherFunc = () => new DefaultStateDispatcher();

        public IComponentContext? ComponentContext { get; set; }

        private bool _sendBackSetting;

        internal ManagerBuilder(WorkspaceSuperviser superviser) => Superviser = superviser;

        public WorkspaceSuperviser Superviser { get; }

        public static RootManager CreateManager(WorkspaceSuperviser superviser, Action<ManagerBuilder> builder)
        {
            var managerBuilder = new ManagerBuilder(superviser);
            builder(managerBuilder);
            return managerBuilder.Build(null);
        }

        public IWorkspaceMapBuilder<TData> WithWorkspace<TData>(Func<WorkspaceBase<TData>> source)
            where TData : class
        {
            var builder = new WorkspaceMapBuilder<TData>(source);
            _states.Add(builder);
            return builder;
        }

        public bool StateRegistrated<TData>(Type state) where TData : class, IStateEntity 
            => _states.OfType<StateBuilder<TData>>().Any(b => b.State == state);

        public IStateBuilder<TData> WithDataSource<TData>(Func<IExtendedDataSource<TData>> source)
            where TData : class, IStateEntity
        {
            var builder = new StateBuilder<TData>(source);
            _states.Add(builder);
            return builder;
        }

        public ManagerBuilder WithDefaultSendback(bool flag)
        {
            _sendBackSetting = flag;
            return this;
        }

        public ManagerBuilder WithMiddleware(Func<IMiddleware> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        public ManagerBuilder WithEffect(Func<IEffect> effect)
        {
            _effects.Add(effect);
            return this;
        }

        public ManagerBuilder WithDispatcher(Func<IStateDispatcherConfigurator>? factory)
        {
            _dispatcherFunc = factory ?? (() => new DefaultStateDispatcher());
            return this;
        }

        public ManagerBuilder WithDispatcher(string name, Func<IStateDispatcherConfigurator>? factory) => throw new NotSupportedException("Polled Dispatcher not Supported");


        internal RootManager Build(AutofacOptions? autofacOptions)
        {
            List<IEffect> additionalEffects = new();
            List<IMiddleware> additionalMiddlewares = new();

            if (ComponentContext != null)
            {
                autofacOptions ??= new AutofacOptions();

                if (autofacOptions.ResolveEffects)
                    additionalEffects.AddRange(ComponentContext.Resolve<IEnumerable<IEffect>>());
                if (autofacOptions.ResolveMiddleware)
                    additionalMiddlewares.AddRange(ComponentContext.Resolve<IEnumerable<IMiddleware>>());
            }

            return new RootManager(Superviser, _dispatcherFunc(), _states,
                _effects.Select(e => e()).Concat(additionalEffects),
                _middlewares.Select(m => m()).Concat(additionalMiddlewares),
                _sendBackSetting, ComponentContext);
        }
    }
}