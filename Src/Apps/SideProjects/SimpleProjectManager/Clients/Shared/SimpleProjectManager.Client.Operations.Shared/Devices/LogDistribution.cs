using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public class LogDistribution
{
    private readonly IActorRef _mediator;

    public LogDistribution(ActorSystem actorSystem)
        => _mediator = DistributedPubSub.Get(actorSystem).Mediator;

    public void Publish(LogBatch batch)
        => _mediator.Tell(new Publish(nameof(LogDistribution), batch));

    public void Subscribe(IActorRef subscriber)
        => _mediator.Tell(new Subscribe(nameof(LogDistribution), subscriber));
}