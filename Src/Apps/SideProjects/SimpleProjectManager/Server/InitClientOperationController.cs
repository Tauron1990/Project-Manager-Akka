using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared;

namespace SimpleProjectManager.Server;

public sealed partial class InitClientOperationController
{
    private readonly ActorSystem _system;
    private readonly NameRegistry _registry;
    private readonly ILogger<InitClientOperationController> _logger;

    public InitClientOperationController(ActorSystem system, NameRegistry registry, ILogger<InitClientOperationController> logger)
    {
        _system = system;
        _registry = registry;
        _logger = logger;
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Error on Initializing Name Registry")]
    private partial void NameRegistryError(Exception ex);
    
    public void Run()
    {
        async void MemberUp()
        {
            try
            {
                var actor = await _registry.Actor;

                ClusterActorDiscovery.Get(_system)
                   .RegisterActor(new ClusterActorDiscoveryMessage.RegisterActor(actor, nameof(NameRegistryFeature)));
            }
            catch (Exception e)
            {
                NameRegistryError(e);
            }
        }

        Cluster.Get(_system).RegisterOnMemberUp(MemberUp);
    }
}