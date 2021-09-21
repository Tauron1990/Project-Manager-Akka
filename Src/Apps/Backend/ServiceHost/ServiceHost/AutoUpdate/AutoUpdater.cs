using Tauron.Features;

namespace ServiceHost.AutoUpdate
{
    public sealed class AutoUpdater : FeatureActorRefBase<IAutoUpdater>, IAutoUpdater
    {
        public AutoUpdater()
            : base("Auto-Updater") { }
    }
}