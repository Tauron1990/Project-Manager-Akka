using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Tauron.Application.Logging.impl;

namespace Tauron.Application.Logging;

[PublicAPI]
public static class LoggingExtensions
{
    public static IHostBuilder ConfigDefaultLogging(this IHostBuilder loggerConfiguration, string applicationName, bool noFile = false)
        => loggerConfiguration.ConfigureLogging((_, builder) => builder.ConfigDefaultLogging(applicationName, noFile));

    public static ILoggingBuilder ConfigDefaultLogging(this ILoggingBuilder loggingBuilder, string applicationName, bool noFile = false)
    {
        ISetupBuilder loggerConfiguration = LogManager.Setup().ConfigDefaultLogging(applicationName, noFile);

        return loggingBuilder.AddNLog(_ => loggerConfiguration.LogFactory);
    }

    public static ISetupBuilder ConfigDefaultLogging(this ISetupBuilder loggerConfiguration, string applicationName, bool noFile = false)
    {
        loggerConfiguration = loggerConfiguration.SetupExtensions(e => e.RegisterLayoutRenderer("event-type", typeof(EventTypeLayoutRenderer)));

        if(noFile)
            return loggerConfiguration;

        const string defaultFile = "default-file";
        const string defaultConsole = "default-Console";

        loggerConfiguration =
            loggerConfiguration.LoadConfiguration(
                b =>
                {
                    b.Configuration.AddTarget(new ColoredConsoleTarget(defaultConsole));
                    b.Configuration.AddTarget(
                        new AsyncTargetWrapper(
                            new FileTarget("actual-" + defaultFile)
                            {
                                Layout = new JsonLayout
                                         {
                                             Attributes =
                                             {
                                                 new JsonAttribute("time", "${longdate}"),
                                                 new JsonAttribute("level", "${level:upperCase=true}"),
                                                 new JsonAttribute("application", applicationName),
                                                 new JsonAttribute("eventType", "${event-type}"),
                                                 new JsonAttribute("message", "${message}"),
                                                 new JsonAttribute(
                                                     "Properties",
                                                     new JsonLayout
                                                     {
                                                         ExcludeEmptyProperties = true,
                                                         ExcludeProperties = new HashSet<string>(StringComparer.Ordinal)
                                                                             {
                                                                                 "time",
                                                                                 "level",
                                                                                 "eventType",
                                                                                 "message"
                                                                             },
                                                         IncludeEventProperties = true
                                                     })
                                             }
                                         },
                                ArchiveAboveSize = 5_242_880,
                                ConcurrentWrites = false,
                                MaxArchiveFiles = 5,
                                FileName = "Logs\\Log.log",
                                ArchiveFileName = "Logs\\Log.{###}.log",
                                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                                EnableArchiveFileCompression = true
                            })
                        {
                            Name = defaultFile
                        });

                    b.Configuration.AddRuleForAllLevels(defaultFile);
                    b.Configuration.AddRuleForAllLevels(defaultConsole);
                });

        return loggerConfiguration;
    }

    public static ISetupBuilder ConfigurateFile(this ISetupBuilder setupBuilder, string applicationName)
    {
        const string defaultFile = "default-file";

        return setupBuilder.LoadConfiguration(
            b =>
            {
                b.Configuration.AddTarget(
                    new AsyncTargetWrapper(
                        new FileTarget("actual-" + defaultFile)
                        {
                            Layout = new JsonLayout
                                     {
                                         Attributes =
                                         {
                                             new JsonAttribute("time", "${longdate}"),
                                             new JsonAttribute("level", "${level:upperCase=true}"),
                                             new JsonAttribute("application", applicationName),
                                             new JsonAttribute("eventType", "${event-type}"),
                                             new JsonAttribute("message", "${message}"),
                                             new JsonAttribute(
                                                 "Properties",
                                                 new JsonLayout
                                                 {
                                                     ExcludeEmptyProperties = true,
                                                     ExcludeProperties = new HashSet<string>(StringComparer.Ordinal)
                                                                         {
                                                                             "time",
                                                                             "level",
                                                                             "eventType",
                                                                             "message"
                                                                         },
                                                     IncludeEventProperties = true
                                                 }),
                                         },
                                     },
                            ArchiveAboveSize = 5_242_880,
                            ConcurrentWrites = false,
                            MaxArchiveFiles = 5,
                            FileName = $"Logs\\{applicationName}.log",
                            ArchiveFileName = $"Logs\\{applicationName}.{{###}}.log",
                            ArchiveNumbering = ArchiveNumberingMode.Rolling,
                            EnableArchiveFileCompression = true,
                        })
                    {
                        Name = defaultFile,
                    });

                b.Configuration.AddRuleForAllLevels(defaultFile);
            });
    }

    public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, Func<ISetupBuilder, ISetupBuilder> config)
        => builder.AddNLog(_ => config(LogManager.Setup()).LogFactory);
}