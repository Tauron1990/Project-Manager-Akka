using Akka.Actor;
using Akka.Cluster;

namespace Tauron.Application.ServiceManager.AppCore.Helper
{
    public sealed class ClusterConnectionTracker : ObservableObject, IClusterConnectionTracker
    {
        private bool _isConnected;

        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }

        public bool IsSelf { get; }

        public AppIp Ip { get; }

        public ClusterConnectionTracker(ActorSystem system, IAppIpManager manager)
        {
            Ip = manager.Ip;
            var cluster = Cluster.Get(system);

            cluster.RegisterOnMemberUp(() => IsConnected = true);
            cluster.RegisterOnMemberRemoved(() => IsConnected = false);

            if (cluster.Settings.SeedNodes.Count != 0) return;
            IsSelf = true;

            if(manager.Ip.IsValid)
                cluster.Join(cluster.SelfAddress);
        }
    }
}