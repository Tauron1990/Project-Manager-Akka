using System.Globalization;
using System.Threading.Tasks;
using Tauron.Application.Logging;
using Tauron.Host;

namespace TimeTracker
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
                                  .ConfigDefaultLogging("Time-Tracker")
                                  .AddModule<MainModule>()
                                  .UseWpf<MainWindow, App>()
                                  .Build().Run();
        }
    }
}