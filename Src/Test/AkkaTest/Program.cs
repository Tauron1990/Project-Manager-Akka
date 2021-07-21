using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Tauron.AkkaHost;

namespace AkkaTest
{
    internal static class Program
    {
        private sealed class TestConsoleApplication : BackgroundService
        {
            private readonly IHostApplicationLifetime _lifetime;
            private readonly ActorSystem _system;
            private readonly ILogger<TestConsoleApplication> _logger;

            public TestConsoleApplication(IHostApplicationLifetime lifetime, ActorSystem system, ILogger<TestConsoleApplication> logger)
            {
                _lifetime = lifetime;
                _system = system;
                _logger = logger;
            }

            protected override Task ExecuteAsync(CancellationToken stoppingToken)
            {
                Task.Factory.StartNew(RunConsole, TaskCreationOptions.LongRunning);
                return Task.CompletedTask;
            }

            private void RunConsole()
            {
                _logger.LogInformation(_system.Name);
                while (true)
                {
                    switch (Console.ReadLine()?.ToLower())
                    {
                        case "end":
                        case "kill":
                        case "x":
                            _lifetime.StopApplication();
                            break;
                        default:
                            Console.WriteLine("Unbekannt");
                            break;
                    }
                }
            }
        }

        private static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .ConfigureAkkaApplication()
                      .ConfigureServices(s =>
                                         {
                                             s.AddOptions();
                                             s.AddSingleton<IHostedService, TestConsoleApplication>();
                                         })
                      .Build().RunAsync();
        }

    }
}