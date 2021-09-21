using System.Globalization;
using Microsoft.Extensions.Logging;
using Tauron.Akka;
using Tauron.Application.Settings;

namespace Akka.MGIHelper.Core.Configuration
{
    public sealed class WindowOptions : ConfigurationBase
    {
        public WindowOptions(IDefaultActorRef<SettingsManager> actor, string scope, ILogger<WindowOptions> logger)
            : base(actor, scope, logger) { }

        public double PositionX
        {
            get => GetValue(double.Parse);
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public double PositionY
        {
            get => GetValue(double.Parse);
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}