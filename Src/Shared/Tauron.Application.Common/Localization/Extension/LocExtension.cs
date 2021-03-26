using Akka.Actor;
using Akka.DependencyInjection;
using Tauron.Localization.Actor;

namespace Tauron.Localization.Extension
{
    public sealed class LocExtension : IExtension
    {
        public IActorRef LocCoordinator { get; private set; } = ActorRefs.Nobody;

        internal LocExtension Init(ActorSystem system)
        {
            LocCoordinator = system.ActorOf(system.GetExtension<ServiceProvider>().Props<LocCoordinator>());
            return this;
        }
    }
}