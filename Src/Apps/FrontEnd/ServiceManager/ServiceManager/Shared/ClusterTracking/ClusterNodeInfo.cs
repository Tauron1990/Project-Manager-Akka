namespace ServiceManager.Shared.ClusterTracking
{
    public sealed record ClusterNodeInfo(string Name, string ServiceType, string Status, string Url);
}