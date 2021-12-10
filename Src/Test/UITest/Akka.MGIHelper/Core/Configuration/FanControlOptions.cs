using Microsoft.Extensions.Logging;
using Tauron.TAkka;
using Tauron.Application.Settings;

namespace Akka.MGIHelper.Core.Configuration
{
    public sealed class FanControlOptions : ConfigurationBase
    {
        public FanControlOptions(IDefaultActorRef<SettingsManager> actor, string scope, ILogger<FanControlOptions> logger)
            : base(actor, scope, logger) { }

        public int ClockTimeMs => GetValue(int.Parse, 1000);

        //set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        public string Ip => GetValue(s => s, "192.168.187.48")!;

        public int GoStandbyTime => GetValue(int.Parse, 25);

        public int MaxStartupTemp => GetValue(int.Parse, 70);

        public int MaxStandbyTemp => GetValue(int.Parse, 115);

        public double FanControlMultipler => GetValue(double.Parse, 1.3d);
    }
}