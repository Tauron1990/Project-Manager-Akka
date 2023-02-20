using System.Threading.Tasks;
using Akka.Hosting;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public sealed class ClusterNodeManagerRef : FeatureActorRefBase<ClusterNodeManagerRef>
    {

        public Task<AllNodesResponse> QueryNodes() => Ask<AllNodesResponse>(new QueryAllNodes());

        public ClusterNodeManagerRef(IRequiredActor<ClusterNodeManagerRef> actor) : base(actor)
        {
        }
    }
}