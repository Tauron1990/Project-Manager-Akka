﻿using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tauron;
using Tauron.AkkaHost;
using Tauron.Application.Logging;

namespace TimeTracker
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
               .ConfigDefaultLogging("Time-Tracker")
               .ConfigureAkkaApplication(
                    ab => ab.ConfigureServices((_, sc) => sc.RegisterModule<MainModule>())
                       .UseWpf<MainWindow, App>())
               .Build().RunAsync().ConfigureAwait(false);
        }
    }
}