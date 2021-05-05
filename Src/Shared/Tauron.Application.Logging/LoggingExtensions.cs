﻿using System.Collections.Generic;
using JetBrains.Annotations;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Tauron.Application.Logging.impl;

namespace Tauron.Application.Logging
{
    [PublicAPI]
    public static class LoggingExtensions
    {
        public static ISetupBuilder ConfigDefaultLogging(this ISetupBuilder loggerConfiguration, string applicationName, bool noFile = false)
        {
            loggerConfiguration.SetupExtensions(e => e.RegisterLayoutRenderer("event-type", typeof(EventTypeLayoutRenderer)));

            if (noFile) return loggerConfiguration;

            const string defaultFile = "default-file";
            loggerConfiguration.LoadConfiguration(b =>
                                                  {
                                                      b.Configuration.AddTarget(new AsyncTargetWrapper(new FileTarget(defaultFile)
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
                                                                                                       }));

                                                      b.Configuration.AddRuleForAllLevels(defaultFile);
                                                  });

            return loggerConfiguration;
        }

    }
}