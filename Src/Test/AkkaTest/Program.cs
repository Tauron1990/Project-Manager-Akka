using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AkkaTest
{
    internal static class Program
    {
        private sealed class TestConsoleApplication : BackgroundService
        {
            private readonly IHostApplicationLifetime _lifetime;

            public TestConsoleApplication(IHostApplicationLifetime lifetime) => _lifetime = lifetime;

            protected override Task ExecuteAsync(CancellationToken stoppingToken)
            {
                Task.Factory.StartNew(RunConsole, TaskCreationOptions.LongRunning);
                return Task.CompletedTask;
            }

            private void RunConsole()
            {
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
                      .ConfigureServices(s => s.AddSingleton<IHostedService, TestConsoleApplication>())
                      .Build().RunAsync();
        }

    }
}