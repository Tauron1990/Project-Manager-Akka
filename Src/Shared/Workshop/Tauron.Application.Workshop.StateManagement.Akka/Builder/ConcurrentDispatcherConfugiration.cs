using Akka.Actor;
using Akka.Routing;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Akka.Builder;

public sealed class ConcurrentDispatcherConfugiration : DispatcherPoolConfigurationBase<IConcurrentDispatcherConfugiration>, IConcurrentDispatcherConfugiration
{
    public override IStateDispatcherConfigurator Create()
        => new ActualDispatcher(Instances, Resizer, SupervisorStrategy, Dispatcher, Custom);

    private sealed class ActualDispatcher : IStateDispatcherConfigurator
    {
        private readonly Func<Props, Props>? _custom;
        private readonly string? _dispatcher;
        private readonly int _instances;
        private readonly Resizer? _resizer;
        private readonly SupervisorStrategy _supervisorStrategy;

        internal ActualDispatcher(
            int instances, Resizer? resizer, SupervisorStrategy supervisorStrategy,
            string? dispatcher, Func<Props, Props>? custom)
        {
            _instances = instances;
            _resizer = resizer;
            _supervisorStrategy = supervisorStrategy;
            _dispatcher = dispatcher;
            _custom = custom;
        }

        private Props Configurate(Props mutator)
        {
            var route = new SmallestMailboxPool(_instances)
               .WithSupervisorStrategy(_supervisorStrategy);

            if (_resizer == null)
                route = route.WithResizer(_resizer);
            if (!string.IsNullOrWhiteSpace(_dispatcher))
                route = route.WithDispatcher(_dispatcher);

            mutator = mutator.WithRouter(route);

            return _custom != null ? _custom(mutator) : mutator;
        }

        public IDriverFactory Configurate(IDriverFactory factory)
        {
            if (factory is not AkkaDriverFactory akkaFactory)
                throw new InvalidOperationException("No Akka Driver Factory Provided");

            return akkaFactory.CustomMutator(Configurate);
        }
    }
}