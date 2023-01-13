using Akka.Actor;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.StateManagement.Dispatcher;

namespace Tauron.Application.Workshop.StateManagement.Akka.Dispatcher.WorkDistributor;

public class WorkDistributorConfigurator : IStateDispatcherConfigurator
{
    private readonly TimeSpan _timeout;

    public WorkDistributorConfigurator(TimeSpan timeout) => _timeout = timeout;

    public IDriverFactory Configurate(IDriverFactory factory)
        => AkkaDriverFactory.Get(factory).CustomMutator(Configurate);

    private Props Configurate(Props mutator)
        => Props.Create(() => new WorkDistributorDispatcher(_timeout));
}