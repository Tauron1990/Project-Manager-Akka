using Akka.Actor;
using Akka.Routing;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Akka.Dispatcher;

[PublicAPI]
public class ConsistentHashStateDispatcher : IStateDispatcherConfigurator
{
    private Props Configurate(Props mutator)
        => mutator.WithRouter(
            new ConsistentHashingPool(2)
               .WithResizer(new DefaultResizer(2, 10)));

    public IDriverFactory Configurate(IDriverFactory factory)
        => AkkaDriverFactory.Get(factory).CustomMutator(Configurate);
}