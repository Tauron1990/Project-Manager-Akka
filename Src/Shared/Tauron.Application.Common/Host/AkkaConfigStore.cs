using System;
using Akka.Configuration;

namespace Tauron.Host
{
    public sealed class AkkaConfigStore
    {
        private readonly object _lock = new();

        public Config Config { get; private set; } = Config.Empty;

        internal void Modify(Func<Config, Config> data)
        {
            lock (_lock) 
                Config = data(Config);
        }
    }
}