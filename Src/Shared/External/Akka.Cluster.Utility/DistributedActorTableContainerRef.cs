using System;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility;

[PublicAPI]
public class DistributedActorTableContainerRef<TKey>
{
    public DistributedActorTableContainerRef(IActorRef target, TimeSpan? timeout = null)
    {
        Target = target;
        Timeout = timeout;
    }

    public IActorRef Target { get; }
    public TimeSpan? Timeout { get; }

    public DistributedActorTableContainerRef<TKey> WithTimeout(TimeSpan? timeout) => new(Target, timeout);

    public Task<DistributedActorTableMessage<TKey>.AddReply> Add(TKey id, IActorRef actor)
        => Target.Ask<DistributedActorTableMessage<TKey>.AddReply>(
            new DistributedActorTableMessage<TKey>.Add(id, actor),
            Timeout);

    public void Remove(TKey id)
    {
        Target.Tell(new DistributedActorTableMessage<TKey>.Remove(id));
    }
}