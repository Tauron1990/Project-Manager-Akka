using Akka.Actor;

namespace Tauron.Features;

public interface IPreparedFeature
{
    void Materialize(in ActorBuilder<GenericState> builder);

    KeyValuePair<Type, object>? InitialState(IUntypedActorContext context);
}