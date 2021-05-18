using System.Collections.Generic;
using System.Linq;
using Akka.Configuration;

namespace Tauron.Application.Master.Commands.Administration.Configuration
{
    public static class AkkaConfigurationBuilder
    {
        public static string PatchSeedUrls(string data, IEnumerable<string> urls)
        {
            var baseConfig = ConfigurationFactory.ParseString(data);
            var newConfig = ConfigurationFactory.ParseString($"akka.cluster.seed-nodes = [{string.Join(',', urls.Select(s => $"\"{s}\""))}]")
                                                .WithFallback(baseConfig);

            return newConfig.ToString(true);
        }
    }
}