using System.Threading.Tasks;
using Tauron.Features;

namespace Tauron.Application.ServiceManager.AppCore.ClusterTracking
{
    public interface IClusterHostManager : IFeatureActorRef<IClusterHostManager>
    {
        Task<MemberChangeSet> GetMemberChangeSet(GetMemberChangeset changeset);
    }
}