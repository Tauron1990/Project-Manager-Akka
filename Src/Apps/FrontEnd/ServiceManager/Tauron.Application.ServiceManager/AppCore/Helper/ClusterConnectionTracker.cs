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

        public ClusterConnectionTracker(ActorSystem system)
        {
            var cluster = Cluster.Get(system);

            cluster.RegisterOnMemberUp(() => IsConnected = true);
            cluster.RegisterOnMemberRemoved(() => IsConnected = false);

            if (cluster.Settings.SeedNodes.Count == 0)
            {
                IsSelf = true;
                cluster.Join(cluster.SelfAddress);
            }
        }
    }
}