using System;
using Tauron.Features;

namespace Tauron.Application.ServiceManager.AppCore.Helper
{
    public sealed class ClusterHostManagerActor : ActorFeatureBase<ClusterHostManagerActor.ClusterState>
    {
        public sealed record ClusterState;

        public static Func<IPreparedFeature> New()
        {
            IPreparedFeature _()
                => Feature.Create(() => new ClusterHostManagerActor(), _ => new ClusterState());

            return _;
        }

        protected override void ConfigImpl()
        {
            
        }
    }
}