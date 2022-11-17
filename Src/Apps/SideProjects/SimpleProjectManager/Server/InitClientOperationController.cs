using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared;

namespace SimpleProjectManager.Server;

public sealed partial class InitClientOperationController
{
    private readonly ILogger<InitClientOperationController> _logger;
    private readonly NameRegistry _registry;
    private readonly ActorSystem _system;

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
                IActorRef actor = await _registry.Actor.ConfigureAwait(false);

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