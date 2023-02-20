using Akka.Actor;

namespace Tauron.Features;

public interface IPreparedFeature
{
    IEnumerable<IFeature<GenericState>?> Materialize();

    KeyValuePair<Type, object>? InitialState(IUntypedActorContext context);
}