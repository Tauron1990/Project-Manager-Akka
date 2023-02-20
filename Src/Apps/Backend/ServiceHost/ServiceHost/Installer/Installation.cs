using Akka.Hosting;
using Tauron.Features;

namespace ServiceHost.Installer
{
    public sealed class Installation : FeatureActorRefBase<Installation>
    {
        public Installation(IRequiredActor<Installation> actor) : base(actor)
        {
        }
    }
}