using JetBrains.Annotations;
using Tauron.Features;

namespace Tauron.Application.ServiceManager.AppCore.Helper
{
    public sealed class ClusterHostManagerRef : FeatureActorRefBase<IClusterHostManager>, IClusterHostManager
    {
        public ClusterHostManagerRef(string? name) 
            : base(name)
        {
        }
    }
}