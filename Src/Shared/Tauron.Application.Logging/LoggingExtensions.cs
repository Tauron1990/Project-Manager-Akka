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

namespace Tauron.Application.Logging
{
    [PublicAPI]
    public static class LoggingExtensions
    {
        public static IHostBuilder ConfigDefaultLogging(this IHostBuilder loggerConfiguration, string applicationName, bool noFile = false)
            => loggerConfiguration.ConfigureLogging((_, builder) => builder.ConfigDefaultLogging(applicationName, noFile));

        public static ILoggingBuilder ConfigDefaultLogging(this ILoggingBuilder loggingBuilder, string applicationName, bool noFile = false)
        {
            var loggerConfiguration = LogManager.Setup().ConfigDefaultLogging(applicationName, noFile);
            
            return loggingBuilder.AddNLog(_ => loggerConfiguration.LogFactory);
        }

        public static ISetupBuilder ConfigDefaultLogging(this ISetupBuilder loggerConfiguration, string applicationName, bool noFile = false)
        {
            loggerConfiguration = loggerConfiguration.SetupExtensions(e => e.RegisterLayoutRenderer("event-type", typeof(EventTypeLayoutRenderer)));

            if (!noFile)
            {

                const string defaultFile = "default-file";
                loggerConfiguration =
                    loggerConfiguration.LoadConfiguration(b =>
                    {
                        b.Configuration.AddTarget(new AsyncTargetWrapper(new FileTarget("actual-" + defaultFile)
                        {
                            Layout = new JsonLayout
                            {
                                Attributes =
                                                                                                                                {
                                                                                                                                    new JsonAttribute("time", "${longdate}"),
                                                                                                                                    new JsonAttribute("level", "$level:upperCase=true"),
                                                                                                                                    new JsonAttribute("application", applicationName),
                                                                                                                                    new JsonAttribute("eventType", "${event-type}"),
                                                                                                                                    new JsonAttribute("message", "${message}"),
                                                                                                                                    new JsonAttribute("Properties",
                                                                                                                                        new JsonLayout
                                                                                                                                        {
                                                                                                                                            ExcludeEmptyProperties = true,
                                                                                                                                            ExcludeProperties = new HashSet<string>
                                                                                                                                                                {
                                                                                                                                                                    "time",
                                                                                                                                                                    "level",
                                                                                                                                                                    "eventType",
                                                                                                                                                                    "message"
                                                                                                                                                                },
                                                                                                                                            IncludeAllProperties = true
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
                    });
            }

            return loggerConfiguration;
        }

        public static ILoggingBuilder AddNLog(this ILoggingBuilder builder, Func<ISetupBuilder, ISetupBuilder> config)
            => builder.AddNLog(_ => config(LogManager.Setup()).LogFactory);
    }
}