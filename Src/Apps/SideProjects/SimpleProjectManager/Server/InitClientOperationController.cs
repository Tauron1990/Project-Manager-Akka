using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared;
using SimpleProjectManager.Client.Operations.Shared.Clustering;

namespace SimpleProjectManager.Server;

public sealed partial class InitClientOperationController
{
    private readonly ILogger<InitClientOperationController> _logger;
    private readonly NameRegistry _registry;
    private readonly ILoggerFactory _factory;
    private readonly ActorSystem _system;

    public InitClientOperationController(ActorSystem system, NameRegistry registry, ILoggerFactory factory)
    {
        _system = system;
        _registry = registry;
        _factory = factory;
        _logger = factory.CreateLogger<InitClientOperationController>();
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Error on Initializing Name Registry")]
    private partial void NameRegistryError(Exception ex);

    public void Run()
    {
        async void MemberUp()
        {
            try
            {
                ClusteringApi.Get(_system).Register(_system.ActorOf(() => new ClusterLogProvider(
                    "Project Manager Server",
                    _factory.CreateLogger<ClusterLogProvider>())));

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