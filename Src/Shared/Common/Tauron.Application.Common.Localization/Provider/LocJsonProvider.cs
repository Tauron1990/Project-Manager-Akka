using Akka.Actor;
using Akka.DependencyInjection;
using Tauron.Application.Common.Localization.Actor;

namespace Tauron.Application.Common.Localization.Provider;

public sealed class LocJsonProvider : ILocStoreProducer
{
    private readonly ActorSystem _actorSystem;

    public LocJsonProvider(ActorSystem actorSystem) => _actorSystem = actorSystem;

    public string Name => "Json";

    public Props GetProps() => _actorSystem.GetExtension<DependencyResolver>().Props<JsonLocLocStoreActor>();
}