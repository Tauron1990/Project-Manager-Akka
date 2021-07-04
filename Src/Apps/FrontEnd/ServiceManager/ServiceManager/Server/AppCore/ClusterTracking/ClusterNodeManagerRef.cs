using System.Threading.Tasks;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public sealed class ClusterNodeManagerRef : FeatureActorRefBase<IClusterNodeManager>, IClusterNodeManager
    {
        public ClusterNodeManagerRef() 
            : base(nameof(ClusterHostManagerActor))
        {
        }

        public Task<MemberChangeSet> GetMemberChangeSet(GetMemberChangeset changeset) => Ask<MemberChangeSet>(changeset);
        public Task<AllNodesResponse> QueryNodes() => Ask<AllNodesResponse>(new QueryAllNodes());
    }
}