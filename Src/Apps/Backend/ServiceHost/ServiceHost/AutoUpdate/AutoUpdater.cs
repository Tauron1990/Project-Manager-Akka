using Akka.Hosting;
using Tauron.Features;

namespace ServiceHost.AutoUpdate
{
    public sealed class AutoUpdater : FeatureActorRefBase<AutoUpdater>
    {
        public AutoUpdater(IRequiredActor<AutoUpdater> actor) : base(actor)
        {
        }
    }
}