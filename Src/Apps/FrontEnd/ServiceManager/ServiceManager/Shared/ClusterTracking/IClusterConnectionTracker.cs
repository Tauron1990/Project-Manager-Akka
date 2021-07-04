namespace ServiceManager.Shared.ClusterTracking
{
    public interface IClusterConnectionTracker : IInternalObject
    {
        string Url { get; }

        bool IsConnected { get; }

        bool IsSelf { get; }

        AppIp Ip { get; }
    }
}