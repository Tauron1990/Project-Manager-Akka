using System.Threading.Tasks;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public interface IClusterNodeManager : IFeatureActorRef<IClusterNodeManager>
    {
        Task<MemberChangeSet> GetMemberChangeSet(GetMemberChangeset changeset);
    }
}