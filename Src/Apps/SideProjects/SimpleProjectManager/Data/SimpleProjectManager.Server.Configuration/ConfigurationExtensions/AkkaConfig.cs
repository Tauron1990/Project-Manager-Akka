﻿using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Akka.Configuration;
using Akka.Hosting;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

public sealed class AkkaConfig : ConfigExtension
{
    public override void Apply(ImmutableDictionary<string, string> propertys, IActorApplicationBuilder applicationBuilder)
    {
        var hoconBuilder = propertys
           .Where(p => p.Key.StartsWith("akka", StringComparison.Ordinal))
           .Aggregate(new StringBuilder(), (builder, pair) => builder.AppendLine(CultureInfo.InvariantCulture, $"{pair.Key}:{pair.Value}"))
           .ToString();

        applicationBuilder.ConfigureAkka((_, b) => b.AddHocon(ConfigurationFactory.ParseString(hoconBuilder), HoconAddMode.Append));
    }
}