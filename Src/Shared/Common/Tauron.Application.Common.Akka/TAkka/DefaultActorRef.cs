using Akka.Actor;

namespace Tauron.TAkka;

public class DefaultActorRef<TActor> : BaseActorRef<TActor>, IDefaultActorRef<TActor>, IDisposable
    where TActor : ActorBase
{
    public DefaultActorRef(ActorRefFactory<TActor> actorBuilder)
        : base(actorBuilder) { }

    public void Dispose() => Actor.Tell(PoisonPill.Instance);
}