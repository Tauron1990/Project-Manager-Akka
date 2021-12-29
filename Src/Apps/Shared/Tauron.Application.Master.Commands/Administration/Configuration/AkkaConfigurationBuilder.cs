using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Akka.Configuration;
using Microsoft.Extensions.Logging;

namespace Tauron.Application.Master.Commands.Administration.Configuration;

public static class AkkaConfigurationBuilder
{
    public const string Base = "base.conf";
    public const string Main = "akka.conf";
    public const string Seed = "seed.conf";
    private static readonly ILogger Log = TauronEnviroment.GetLogger(typeof(AkkaConfigurationBuilder));

    public static IObservable<string> PatchSeedUrls(string data, IEnumerable<string> urls)
    {
        var baseConfig = ConfigurationFactory.ParseString(data);
        var newConfig = ConfigurationFactory.ParseString($"akka.cluster.seed-nodes = [{string.Join(',', urls.Select(s => $"\"{s}\""))}]")
           .WithFallback(baseConfig);

        return Observable.Return(newConfig.ToString(includeFallback: true));
    }

    public static string ApplyMongoUrl(string dat, string baseConfig, string url)
    {
        Log.LogInformation("Update AppBase Configuration");

        const string snapshot = "akka.persistence.snapshot-store.plugin = \"akka.persistence.snapshot-store.mongodb\"";
        const string journal = "akka.persistence.journal.plugin = \"akka.persistence.journal.mongodb\"";

        const string connectionSnapshot = "akka.persistence.snapshot-store.mongodb.connection-string = \"{0}\"";
        const string connectionJournal = "akka.persistence.journal.mongodb.connection-string = \"{0}\"";

        var currentConfiguration = ConfigurationFactory.ParseString(dat);

        var hasBase = currentConfiguration.HasPath("akka.persistence.journal.mongodb.connection-string ")
                   || currentConfiguration.HasPath("akka.persistence.snapshot-store.mongodb.connection-string");

        if (!hasBase)
        {
            Log.LogInformation("Apply Default Configuration");
            currentConfiguration = ConfigurationFactory.ParseString(baseConfig).WithFallback(currentConfiguration);
        }

        var builder = new StringBuilder();

        builder
           .AppendLine(snapshot)
           .AppendLine(journal)
           .AppendFormat(connectionSnapshot, url).AppendLine()
           .AppendFormat(connectionJournal, url).AppendLine();

        currentConfiguration = ConfigurationFactory.ParseString(builder.ToString()).WithFallback(currentConfiguration);

        Log.LogInformation("AppBase Configuration Updated");

        return currentConfiguration.ToString(includeFallback: true);
    }
}