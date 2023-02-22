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
using LogLevel = NLog.LogLevel;

namespace Tauron.Application.Logging;

[PublicAPI]
public static class LoggingExtensions
{
    public static IHostBuilder ConfigDefaultLogging(this IHostBuilder loggerConfiguration, string applicationName, bool noFile = false)
        => loggerConfiguration.ConfigureLogging((_, builder) => builder.ConfigDefaultLogging(applicationName, noFile));

    public static ILoggingBuilder ConfigDefaultLogging(this ILoggingBuilder loggingBuilder, string applicationName, bool noFile = false)
    {
        LogManager.Setup(s => s.ConfigDefaultLogging(applicationName, noFile));

        return loggingBuilder.AddNLog();
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
                    b.Configuration.AddTarget(new ColoredConsoleTarget(defaultConsole)
                    {
                        UseDefaultRowHighlightingRules = false,
                        RowHighlightingRules =
                        {
                            //LogLevel.Debug
                            new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.White, ConsoleOutputColor.Black),
                            //LogLevel.Trace
                            new ConsoleRowHighlightingRule("level == LogLevel.Trace", ConsoleOutputColor.White, ConsoleOutputColor.Black),
                            //LogLevel.Info
                            new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.Blue, ConsoleOutputColor.Black),
                            //LogLevel.Warn
                            new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.Black),
                            //LogLevel.Error
                            new ConsoleRowHighlightingRule("level == LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.Black),
                            //LogLevel.Fatal
                            new ConsoleRowHighlightingRule("level == LogLevel.Fatal", ConsoleOutputColor.DarkYellow, ConsoleOutputColor.DarkRed),
                        },
                    });
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
                                                 new JsonAttribute("message", "${message} ${onexception:EXCEPTION OCCURRED\\:${exception:format=type,message,method:maxInnerExceptionLevel=5:innerFormat=shortType,message,method}}"),
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
                                                                                 "message",
                                                                             },
                                                         IncludeEventProperties = true,
                                                     }),
                                             },
                                         },
                                ArchiveAboveSize = 5_242_880,
                                ConcurrentWrites = false,
                                MaxArchiveFiles = 5,
                                FileName = "Logs\\Log.log",
                                ArchiveFileName = "Logs\\Log.{###}.log",
                                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                            })
                        {
                            Name = defaultFile,
                        });

                    b.Configuration.AddRule(LogLevel.Info, LogLevel.Fatal, defaultFile);
                    b.Configuration.AddRule(LogLevel.Info, LogLevel.Fatal, defaultConsole);
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
                                                                             "message",
                                                                         },
                                                     IncludeEventProperties = true,
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