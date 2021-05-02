using Tauron.Akka;
using Tauron.Features;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class AppRegistry : FeatureActorRefBase<IAppRegistry>, IAppRegistry
    {
        public AppRegistry() : base("Apps-Registry") {} 
    }
}