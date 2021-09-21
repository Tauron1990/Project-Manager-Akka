using Microsoft.Extensions.Logging;
using Tauron.Akka;
using Tauron.Application.Settings;

namespace Akka.MGIHelper.Core.Configuration
{
    public class ProcessConfig : ConfigurationBase
    {
        public ProcessConfig(IDefaultActorRef<SettingsManager> actor, string scope, ILogger<ProcessConfig> logger)
            : base(actor, scope, logger) { }

        public string Kernel => GetValue(s => s, string.Empty)!;

        public string Client => GetValue(s => s, string.Empty)!;
    }
}