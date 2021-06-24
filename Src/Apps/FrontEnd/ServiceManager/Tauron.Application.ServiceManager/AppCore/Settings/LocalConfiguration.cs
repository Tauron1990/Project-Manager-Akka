using Tauron.Akka;
using Tauron.Application.Settings;

namespace Tauron.Application.ServiceManager.AppCore.Settings
{
    public sealed class LocalConfiguration : ConfigurationBase, ILocalConfiguration
    {
        public LocalConfiguration(IDefaultActorRef<SettingsManager> actor) : base(actor, LocalConfigurationProvider.LocalScope)
        {
        }

        public string DatabaseUrl
        {
            get => GetValue(s => s, string.Empty)!;
            set => SetValue(value);
        }
    }
}