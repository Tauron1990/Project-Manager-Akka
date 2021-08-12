using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace AkkaTest.FusionTest.Helper
{
    public static class ConnectionHelper
    {
        public static async Task<bool> Connect(ActorSystem system, IHostApplicationLifetime lifetime)
        {
            try
            {
                Console.Title = "Fusion Test Cluster Verbindung";
                AnsiConsole.Clear();
                AnsiConsole.Render(new Rule("Cluster Beitreten"));
                AnsiConsole.WriteLine();
                var url = await Task.Run(() => AnsiConsole.Ask<string>("Url zum Cluster Seed: "));
                var cluster = Cluster.Get(system);
                await cluster.JoinAsync(Address.Parse(url));

                return true;
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteException(e);
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine("Fehler beim Starten");
                AnsiConsole.WriteLine("Beende");
                await Task.Delay(10000);
                lifetime.StopApplication();

                return false;
            }
        }
    }
}