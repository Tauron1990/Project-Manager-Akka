using System.Globalization;
using System.Threading.Tasks;
using Tauron.Host;

namespace TimeTracker
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
                                  .AddModule<MainModule>()
                                  .UseWpf<MainWindow, App>()
                                  .Build().Run();
        }
    }
}