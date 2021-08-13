using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Shared;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;

namespace ServiceManager.Server
{
    public static class Program
    {
        public static readonly string ExeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;

        private static readonly RestartHelper RestartHelper = new();
        private static readonly AppIpManager AppIpManager = new();

        public static async Task Main(string[] args)
        {
            await AppIpManager.Aquire();
            await CreateHostBuilder(args).Build().RunAsync();

            if (!RestartHelper.Restart) return;

            #if RELSEASE
            var file = Path.ChangeExtension(Assembly.GetEntryAssembly()?.Location, ".exe");
            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                Process.Start(file);

            #endif
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(sc => sc.AddSingleton<IRestartHelper>(RestartHelper).AddSingleton<IInternalAppIpManager>(AppIpManager))
                .StartNode(KillRecpientType.Frontend, IpcApplicationType.NoIpc, consoleLog: true)
                .ConfigureWebHostDefaults(
                     webBuilder =>
                     {
                         if (AppIpManager.Ip.IsValid)
                             webBuilder.UseUrls("http://localhost:83");//, $"http://{AppIpManager.Ip.Ip}:81");
                         else
                             webBuilder.UseUrls("http://localhost:83");
                         webBuilder.UseStartup<Startup>();
                     });
    }
}
