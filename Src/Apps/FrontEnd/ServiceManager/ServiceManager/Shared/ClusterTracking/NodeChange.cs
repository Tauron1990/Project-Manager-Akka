namespace ServiceManager.Shared.ClusterTracking
{
    public sealed record NodeChange(ClusterNodeInfo Info, bool Remove);
}