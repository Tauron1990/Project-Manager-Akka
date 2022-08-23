using Akka.Actor;
using Akka.Cluster;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

public sealed class ClusterJoinSelf
{
    private readonly ActorSystem _system;

    public ClusterJoinSelf(ActorSystem system)
        => _system = system;

    public void Run()
        => Cluster.Get(_system).Join(Cluster.Get(_system).SelfAddress);
}