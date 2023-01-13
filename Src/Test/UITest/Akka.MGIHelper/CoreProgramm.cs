using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;
using Tauron.AkkaHost;
using Tauron.Application.Logging;

namespace Akka.MGIHelper
{
    public static class CoreProgramm
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
               .ConfigureLogging(lb => lb.AddNLog(sb => sb.ConfigDefaultLogging("MGI_Helper").LoadConfiguration(e => e.Configuration.AddRuleForAllLevels(new LoggerViewerSink()))))
               .ConfigureAkkaApplication(
                    ab => ab.AddModule<MainModule>()
                       .ConfigureAkkaSystem((_, s) => s.RegisterLocalization())
                       .UseWpf<MainWindow, App>())
               .Build().RunAsync();
        }
    }
}