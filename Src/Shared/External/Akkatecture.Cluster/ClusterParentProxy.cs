using Akka.Actor;
using Akkatecture.Messages;

namespace Akkatecture.Cluster;

public class ClusterParentProxy : ReceiveActor
{
    private readonly IActorRef _child;

    #pragma warning disable AV1564
    public ClusterParentProxy(Props childProps, bool shouldUnsubscribe = true)
        #pragma warning restore AV1564
    {
        _child = Context.ActorOf(childProps);

        if(shouldUnsubscribe)
            _child.Tell(UnsubscribeFromAll.Instance);

        ReceiveAny(Forward);
    }

    public void Forward(object message)
        => _child.Forward(message);
}