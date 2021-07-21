using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tauron.Application.Logging;
using Tauron.AkkaHost;

namespace TimeTracker
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                      .ConfigDefaultLogging("Time-Tracker")
                      .ConfigureAkkaApplication(ab => ab.AddModule<MainModule>()
                                                        .UseWpf<MainWindow, App>())
                      .Build().RunAsync();
        }
    }
}