using Tauron.Features;

namespace ServiceHost.Services
{
    public sealed class AppManager : FeatureActorRefBase<IAppManager>, IAppManager
    {
        public AppManager() 
            : base("Service-Manager")
        {
        }
    }
}