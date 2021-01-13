using System;
using AnyConsole;

namespace Tauron.Application.AkkaNode.Bootstrap.Console
{
    public static class ConsoleConfiguration
    {
        private static Action<ExtendedConsoleConfiguration>? _configuration;

        internal static Action<ExtendedConsoleConfiguration> Configuration => _configuration ?? (c => c.SetUpdateInterval(TimeSpan.FromMilliseconds(200)));

        public static void AddConfiguration(Action<ExtendedConsoleConfiguration> config) => _configuration = config.Combine(_configuration);
    }
}