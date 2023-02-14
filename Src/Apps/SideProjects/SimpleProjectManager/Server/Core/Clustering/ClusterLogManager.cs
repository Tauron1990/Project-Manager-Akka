using Akka.Actor;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared.Clustering;

namespace SimpleProjectManager.Server.Core.Clustering;

public sealed class ClusterLogManager : ReceiveActor
{
    private readonly List<IActorRef> _logProviders = new();

    public ClusterLogManager()
    {
        Receive<ClusterActorDiscoveryMessage.ActorUp>(up => _logProviders.Add(up.Actor));
        Receive<ClusterActorDiscoveryMessage.ActorDown>(down => _logProviders.Remove(down.Actor));

        Receive<QueryLogFileNames>(QueryNames);
    }

    private void QueryNames(QueryLogFileNames obj)
    {
        
    }

    protected override void PreStart()
    {
        ClusteringApi.Get(Context.System).Subscribe();
        base.PreStart();
    }
}

internal sealed class FileTracker
{
    
}