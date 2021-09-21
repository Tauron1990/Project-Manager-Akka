using System;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Tauron.Akka;
using Tauron.Application.Settings;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class AppConfig : ConfigurationBase
    {
        private ImmutableList<string>? _renctFiles;

        public AppConfig(IDefaultActorRef<SettingsManager> actor, string scope, ILogger<AppConfig> logger)
            : base(actor, scope, logger) { }

        public ImmutableList<string> RenctFiles
        {
            get
            {
                return _renctFiles ??= ImmutableList<string>.Empty.AddRange(
                    GetValue(s => s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries), Array.Empty<string>())!);
            }
            set
            {
                _renctFiles = value;
                SetValue(string.Join(';', _renctFiles));
            }
        }
    }
}