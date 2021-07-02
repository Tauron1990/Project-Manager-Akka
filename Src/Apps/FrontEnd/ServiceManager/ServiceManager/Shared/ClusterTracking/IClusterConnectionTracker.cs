namespace ServiceManager.Shared.ClusterTracking
{
    public interface IClusterConnectionTracker : IInternalObject
    {
        bool IsConnected { get; }

        bool IsSelf { get; }

        AppIp Ip { get; }
    }
}