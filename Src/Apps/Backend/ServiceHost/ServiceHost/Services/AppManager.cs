using Akka.Hosting;
using Tauron.Features;

namespace ServiceHost.Services
{
    public sealed class AppManager : FeatureActorRefBase<AppManager>
    {
        public AppManager(IRequiredActor<AppManager> actor) : base(actor)
        {
        }
    }
}