﻿using System.Threading.Tasks;
using Autofac;
using NLog;
using Tauron.Application.Logging;
using Tauron.Application.Wpf.SerilogViewer;
using Tauron.Host;
using Tauron.Localization;

namespace Akka.MGIHelper
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            var builder = ActorApplication.Create(args);

            builder
                .ConfigureLogging((_, configuration) => configuration.ConfigDefaultLogging("MGI_Helper").LoadConfiguration(e => e.Configuration.AddRuleForAllLevels(new LoggerViewerSink())))
                .ConfigureAutoFac(cb => cb.RegisterModule<MainModule>())
                .ConfigurateAkkaSystem((_, system) => system.RegisterLocalization())
               .UseWpf<MainWindow, App>();

            using var app = builder.Build();
            await app.Run();
        }
    }
}