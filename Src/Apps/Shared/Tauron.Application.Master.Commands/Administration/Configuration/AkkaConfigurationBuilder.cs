using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Akka.Configuration;

namespace Tauron.Application.Master.Commands.Administration.Configuration
{
    public static class AkkaConfigurationBuilder
    {
        public const string Base = "base.conf";
        public const string Main = "akka.conf";
        public const string Seed = "seed.conf";

        public static IObservable<string> PatchSeedUrls(string data, IEnumerable<string> urls)
        {
            var baseConfig = ConfigurationFactory.ParseString(data);
            var newConfig = ConfigurationFactory.ParseString($"akka.cluster.seed-nodes = [{string.Join(',', urls.Select(s => $"\"{s}\""))}]")
                                                .WithFallback(baseConfig);

            return Observable.Return(newConfig.ToString(true));
        }
    }
}