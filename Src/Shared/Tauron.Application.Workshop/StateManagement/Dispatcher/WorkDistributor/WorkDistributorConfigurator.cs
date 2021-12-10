using System;
using Akka.Actor;

namespace Tauron.Application.Workshop.StateManagement.Dispatcher.WorkDistributor;

public class WorkDistributorConfigurator : IStateDispatcherConfigurator
{
    private readonly TimeSpan _timeout;

    public WorkDistributorConfigurator(TimeSpan timeout) => _timeout = timeout;

    public Props Configurate(Props mutator)
        => Props.Create(() => new WorkDistributorDispatcher(_timeout));
}