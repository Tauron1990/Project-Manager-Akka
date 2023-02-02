using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed partial class LogDistribution
{
    private readonly ILogger<LogDistribution> _logger = LoggingProvider.LoggerFactory.CreateLogger<LogDistribution>();
    private readonly DistributedPubSub _distributedPubSub;

    public LogDistribution(ActorSystem actorSystem)
        => _distributedPubSub = DistributedPubSub.Get(actorSystem);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "PubSub Mediator for Logdistribution Terminated")]
    private partial void MediatorTerminated();
    
    public void Publish(LogBatch batch)
    {
        if(_distributedPubSub.IsTerminated)
            MediatorTerminated();
        _distributedPubSub.Mediator.Tell(new Publish(nameof(LogDistribution), batch));
    }

    public void Subscribe(IActorRef subscriber)
    {
        if(_distributedPubSub.IsTerminated)
            MediatorTerminated();
        _distributedPubSub.Mediator.Tell(new Subscribe(nameof(LogDistribution), subscriber));
    }
}