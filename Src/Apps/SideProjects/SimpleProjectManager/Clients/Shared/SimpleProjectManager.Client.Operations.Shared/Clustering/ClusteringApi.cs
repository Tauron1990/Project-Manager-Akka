using Akka.Actor;
using Akka.Cluster.Utility;

namespace SimpleProjectManager.Client.Operations.Shared.Clustering;

public class ClusteringApi
{
    private readonly ClusterActorDiscovery _actorDiscovery;

    private ClusteringApi(ClusterActorDiscovery actorDiscovery) => _actorDiscovery = actorDiscovery;

    public static ClusteringApi Get(ActorSystem actorSystem) => new(ClusterActorDiscovery.Get(actorSystem));
    
    public void Subscribe() 
        => _actorDiscovery.MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ClusteringApi)));

    public void Register(IActorRef actor)
        => _actorDiscovery.RegisterActor(new ClusterActorDiscoveryMessage.RegisterActor(actor, nameof(ClusteringApi)));
}