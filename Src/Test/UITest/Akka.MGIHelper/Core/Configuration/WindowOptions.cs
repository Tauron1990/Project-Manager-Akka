using System.Globalization;
using Tauron.Akka;
using Tauron.Application.Settings;

namespace Akka.MGIHelper.Core.Configuration
{
    public sealed class WindowOptions : ConfigurationBase
    {
        public WindowOptions(IDefaultActorRef<SettingsManager> actor, string scope)
            : base(actor, scope)
        {
        }

        public double PositionX
        {
            get => GetValue(double.Parse)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public double PositionY
        {
            get => GetValue(double.Parse)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}