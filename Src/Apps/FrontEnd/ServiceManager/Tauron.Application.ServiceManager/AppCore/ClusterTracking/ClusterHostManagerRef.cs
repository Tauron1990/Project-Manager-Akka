using Tauron.Features;

namespace Tauron.Application.ServiceManager.AppCore.ClusterTracking
{
    public sealed class ClusterHostManagerRef : FeatureActorRefBase<IClusterHostManager>, IClusterHostManager
    {
        public ClusterHostManagerRef(string? name) 
            : base(name)
        {
        }
    }
}