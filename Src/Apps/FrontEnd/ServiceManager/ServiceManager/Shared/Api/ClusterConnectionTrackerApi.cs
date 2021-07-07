namespace ServiceManager.Shared.Api
{
    public class ClusterConnectionTrackerApi
    {
        public const string ClusterConnectionTracker = "api/clusterconnectiontracker";

        public const string IsSelf = ClusterConnectionTracker + "/" + nameof(IsSelf);

        public const string IsConnected = ClusterConnectionTracker + "/" + nameof(IsConnected);

        public const string SelfUrl = ClusterConnectionTracker + "/" + nameof(SelfUrl);

        public const string ConnectToCluster = ClusterConnectionTracker + "/" + nameof(ConnectToCluster);
    }
}