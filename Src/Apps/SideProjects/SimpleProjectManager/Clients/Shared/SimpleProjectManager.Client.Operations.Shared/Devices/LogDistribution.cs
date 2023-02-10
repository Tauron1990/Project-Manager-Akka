using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Cluster.Utility;
using Akka.Util.Internal;
using SimpleProjectManager.Shared.Services.Devices;

namespace SimpleProjectManager.Client.Operations.Shared.Devices;

public sealed class LogDistribution
{
    private readonly IActorRef _distributedPubSub;

    public LogDistribution(IActorRefFactory actorSystem)
        => _distributedPubSub = actorSystem.ActorOf<LogDistributionActor>();
    
    public void Publish(LogBatch batch)
        => _distributedPubSub.Tell(new Publish(nameof(LogDistribution), batch));

    public void Subscribe(IActorRef subscriber) 
        => _distributedPubSub.Tell(new Subscribe(nameof(LogDistribution), subscriber));

    private sealed class LogDistributionActor : ReceiveActor
    {
        private readonly List<IActorRef> _actors = new();
        private readonly List<IActorRef> _subscriptions = new();

#pragma warning disable GU0073
        public LogDistributionActor()
#pragma warning restore GU0073
        {
            Receive<Terminated>(terminated =>
            {
                _actors.Remove(terminated.ActorRef);
                _subscriptions.Remove(terminated.ActorRef);
            });

            Receive<ClusterActorDiscoveryMessage.ActorUp>(
                up =>
                {
                    if(up.Actor.Equals(Self)) return;
                    
                    _actors.Add(up.Actor);
                    Context.Watch(up.Actor);
                });
            Receive<ClusterActorDiscoveryMessage.ActorDown>(down => _actors.Remove(down.Actor));

            Receive<Subscribe>(
                sub =>
                {
                    _subscriptions.Add(sub.Ref);
                    Context.Watch(sub.Ref);
                    
                    sub.Ref.Tell(new SubscribeAck(sub));
                });

            Receive<Publish>(Handler);
        }

        private void Handler(Publish p)
        {
            _subscriptions.ForEach(sub => sub.Tell(p.Message));
            _actors.Where(act => !act.Equals(Sender)).ForEach(act => act.Tell(p));
        }

        protected override void PreStart()
        {
            ClusterActorDiscovery disc = ClusterActorDiscovery.Get(Context.System);
            disc.MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(nameof(LogDistribution)));
            disc.RegisterActor(new ClusterActorDiscoveryMessage.RegisterActor(Self, nameof(LogDistribution)));
            base.PreStart();
        }

        protected override void PostStop()
        {
            ClusterActorDiscovery disc = ClusterActorDiscovery.Get(Context.System);
            disc.UnMonitorActor(new ClusterActorDiscoveryMessage.UnmonitorActor(nameof(LogDistribution)));
            disc.UnRegisterActor(new ClusterActorDiscoveryMessage.UnregisterActor(Self));
            base.PostStop();
        }
    }
}