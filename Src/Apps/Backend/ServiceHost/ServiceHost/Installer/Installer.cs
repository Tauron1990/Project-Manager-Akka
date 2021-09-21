using Tauron.Features;

namespace ServiceHost.Installer
{
    public sealed class Installer : FeatureActorRefBase<IInstaller>, IInstaller
    {
        public Installer() : base("Installer") { }
    }
}