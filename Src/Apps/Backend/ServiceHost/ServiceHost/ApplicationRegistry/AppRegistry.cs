using Akka.Hosting;
using Tauron.Features;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class AppRegistry : FeatureActorRefBase<AppRegistry>
    {
        public AppRegistry(IRequiredActor<AppRegistry> actor) : base(actor)
        {
        }
    }
}