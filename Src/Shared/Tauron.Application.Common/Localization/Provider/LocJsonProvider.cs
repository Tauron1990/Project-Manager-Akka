using Akka.Actor;
using Akka.DependencyInjection;
using Tauron.Localization.Actor;

namespace Tauron.Localization.Provider
{
    public sealed class LocJsonProvider : ILocStoreProducer
    {
        private readonly ActorSystem _actorSystem;

        public LocJsonProvider(ActorSystem actorSystem) => _actorSystem = actorSystem;

        public string Name => "Json";

        public Props GetProps() => _actorSystem.GetExtension<ServiceProvider>().Props<JsonLocLocStoreActor>();
    }
}