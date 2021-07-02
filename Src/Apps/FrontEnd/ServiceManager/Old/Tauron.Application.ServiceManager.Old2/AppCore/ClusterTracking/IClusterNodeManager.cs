using System.Threading.Tasks;
using Tauron.Features;

namespace Tauron.Application.ServiceManager.AppCore.ClusterTracking
{
    public interface IClusterNodeManager : IFeatureActorRef<IClusterNodeManager>
    {
        Task<MemberChangeSet> GetMemberChangeSet(GetMemberChangeset changeset);
    }
}