using System;
using Akka.Actor;
using Akka.Routing;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Builder
{
    [PublicAPI]
    public abstract class DispatcherPoolConfigurationBase<TConfig> : IDispatcherPoolConfiguration<TConfig>
        where TConfig : class, IDispatcherPoolConfiguration<TConfig>
    {
        protected Func<Props, Props>? Custom;
        protected string? Dispatcher;
        protected int Instances = 2;
        protected Resizer? Resizer;
        protected SupervisorStrategy SupervisorStrategy = Pool.DefaultSupervisorStrategy;

        public TConfig NrOfInstances(int number)
        {
            Instances = number;
            return (this as TConfig)!;
        }

        public TConfig WithSupervisorStrategy(SupervisorStrategy strategy)
        {
            SupervisorStrategy = strategy;
            return (this as TConfig)!;
        }

        public TConfig WithResizer(Resizer resizer)
        {
            Resizer = resizer;
            return (this as TConfig)!;
        }

        public TConfig WithAkkaDispatcher(string name)
        {
            Dispatcher = name;
            return (this as TConfig)!;
        }

        public TConfig WithCustomization(Func<Props, Props> custom)
        {
            Custom = Custom.Combine(custom);
            return (this as TConfig)!;
        }

        public abstract IStateDispatcherConfigurator Create();
    }
}