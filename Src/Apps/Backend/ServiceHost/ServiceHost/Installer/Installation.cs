using Tauron.Features;

namespace ServiceHost.Installer
{
    public sealed class Installation : FeatureActorRefBase<IInstaller>, IInstaller
    {
    }
}