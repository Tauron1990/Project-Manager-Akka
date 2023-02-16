using Akka.Actor;
using Akka.Hosting;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
public abstract class FeatureActorRefBase<TInterface> : IFeatureActorRef<TInterface>
    where TInterface : IFeatureActorRef<TInterface>
{
    private readonly IRequiredActor<TInterface> _actor;

    protected FeatureActorRefBase(IRequiredActor<TInterface> actor) => _actor = actor;

    public IActorRef Actor => _actor.ActorRef;

    public TInterface Tell(object msg)
    {
        _actor.ActorRef.Tell(msg);

        return (TInterface)(object)this;
    }

    public TInterface Forward(object msg)
    {
        _actor.ActorRef.Forward(msg);

        return (TInterface)(object)this;
    }

    public async Task<TResult> Ask<TResult>(object msg, TimeSpan? timeout = null)
    {
        return await _actor.ActorRef.Ask<TResult>(msg, timeout).ConfigureAwait(false);
    }

    public void Tell(object message, IActorRef sender)
        => _actor.ActorRef.Tell(message, sender);
}