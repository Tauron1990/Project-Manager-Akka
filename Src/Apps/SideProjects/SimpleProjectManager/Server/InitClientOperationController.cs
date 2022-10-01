using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared;

namespace SimpleProjectManager.Server;

public sealed class InitClientOperationController
{
    private readonly ActorSystem _system;
    private readonly NameRegistry _registry;
    private readonly ILoggerFactory _loggerFactory;

    public InitClientOperationController(ActorSystem system, NameRegistry registry, ILoggerFactory loggerFactory)
    {
        _system = system;
        _registry = registry;
        _loggerFactory = loggerFactory;

    }
    
    public void Run()
    {
        Cluster.Get(_system).RegisterOnMemberUp(
            () =>
            {
                ClusterActorDiscovery.Get(_system)
                   .RegisterActor(new ClusterActorDiscoveryMessage.RegisterActor(_registry.Actor, nameof(NameRegistryFeature)));
            });
    }
}