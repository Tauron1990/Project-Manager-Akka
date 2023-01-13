using Akka.Actor;
using Akka.Routing;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Akka.Dispatcher;

[PublicAPI]
public sealed class ConcurrentStateDispatcher : IStateDispatcherConfigurator
{
    public IDriverFactory Configurate(IDriverFactory factory)
        => AkkaDriverFactory.Get(factory).CustomMutator(Configurate);

    private Props Configurate(Props mutator) => mutator.WithRouter(
        new SmallestMailboxPool(
            2,
            new DefaultResizer(2, 10),
            Pool.DefaultSupervisorStrategy,
            mutator.Dispatcher));
}