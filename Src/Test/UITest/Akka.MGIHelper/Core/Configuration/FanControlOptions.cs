using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.TAkka;
using Tauron.Application.Settings;

namespace Akka.MGIHelper.Core.Configuration
{
    public sealed class FanControlOptions : ConfigurationBase
    {
        public FanControlOptions(IDefaultActorRef<SettingsManager> actor, string scope, ILogger<FanControlOptions> logger)
            : base(actor, scope, logger) { }

        public int ClockTimeMs
        {
            get => GetValue(int.Parse, 1000);
            [UsedImplicitly]
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        //set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        public string Ip
        {
            get => GetValue(s => s, "192.168.187.48")!;
            [UsedImplicitly]
            set => SetValue(value);
        }

        public int GoStandbyTime
        {
            get => GetValue(int.Parse, 25);
            [UsedImplicitly]
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public int MaxStartupTemp
        {
            get => GetValue(int.Parse, 70);
            [UsedImplicitly]
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public int MaxStandbyTemp
        {
            get => GetValue(int.Parse, 115);
            [UsedImplicitly]
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public double FanControlMultipler
        {
            get => GetValue(double.Parse, 1.3d);
            [UsedImplicitly]
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}