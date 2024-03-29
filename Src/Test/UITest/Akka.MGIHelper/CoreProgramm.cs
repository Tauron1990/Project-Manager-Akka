﻿using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tauron;
using Tauron.AkkaHost;
using Tauron.Application.Logging;
using Tauron.Localization;

namespace Akka.MGIHelper
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .RegisterModule<MainModule>()
                .ConfigureLogging(
                    lb => lb.AddNLog(
                        sb => sb.ConfigDefaultLogging("MGI_Helper")))
                .ConfigureAkkaApplication(
                    ab => ab
                        .ConfigureAkka((_, c) => c.AddStartup((system, _) => system.RegisterLocalization()))
                        .UseWpf<MainWindow, App>())
                .Build().RunAsync().ConfigureAwait(false);
        }
    }
}