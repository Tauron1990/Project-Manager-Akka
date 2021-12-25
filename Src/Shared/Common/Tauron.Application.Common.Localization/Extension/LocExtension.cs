using Akka.Actor;
using Akka.DependencyInjection;
using Tauron.Application.Common.Localization.Actor;

namespace Tauron.Application.Common.Localization.Extension;

public sealed class LocExtension : IExtension
{
    public IActorRef LocCoordinator { get; private set; } = ActorRefs.Nobody;

    internal LocExtension Init(ActorSystem system)
    {
        LocCoordinator = system.ActorOf(system.GetExtension<DependencyResolver>().Props<LocCoordinator>());

        return this;
    }
}