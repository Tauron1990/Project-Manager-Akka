using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Tauron.Application.AkkNode.Services.Configuration;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Bootstrap
{
    public static partial class Bootstrap
    {
        [PublicAPI]
        public static IApplicationBuilder ConfigurateNode(this IApplicationBuilder builder)
        {
            var config = GetConfig();

            return builder
               .ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.Add(
                    new HoconConfigurationSource(() => config, 
                        ("akka.appinfo.applicationName", "applicationName"),
                        ("akka.appinfo.actorsystem", "actorsystem"),
                        ("akka.appinfo.appslocation", "AppsLocation"))))
               .ConfigureAkka(_ => config)
               .ConfigureLogging((context, configuration) =>
                {
                    configuration.WriteTo.File(new CompactJsonFormatter(), "Logs\\Log.log", fileSizeLimitBytes: 5_242_880, retainedFileCountLimit: 2);

                    configuration
                       .MinimumLevel.Debug()
                       .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                       .MinimumLevel.Override("System", LogEventLevel.Warning)
                       .Enrich.WithProperty("ApplicationName", context.HostEnvironment.ApplicationName)
                       .Enrich.FromLogContext()
                       .Enrich.WithExceptionDetails()
                       .Enrich.With<EventTypeEnricher>()
                       .Enrich.With<LogLevelWriter>();
                });
        }

        private static Config GetConfig()
        {
            const string baseconf = "base.conf";
            const string main = "akka.conf";
            const string seed = "seed.conf";

            var config = Config.Empty;

            if (File.Exists(baseconf))
                config = ConfigurationFactory.ParseString(File.ReadAllText(baseconf)).WithFallback(config);

            if (File.Exists(main))
                config = ConfigurationFactory.ParseString(File.ReadAllText(main)).WithFallback(config);

            if (File.Exists(seed))
                config = ConfigurationFactory.ParseString(File.ReadAllText(seed)).WithFallback(config);

            return config;
        }

        private class LogLevelWriter : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("Level", new ScalarValue(logEvent.Level)));
            }
        }
    }
}