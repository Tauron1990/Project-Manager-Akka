using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public sealed record QueryAllNodes;

    public sealed record AllNodesResponse(ClusterNodeInfo[] Infos);
}