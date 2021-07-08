using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Shared;
using Tauron.Application.AspIntegration;

namespace ServiceManager.Server
{
    public class Program
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
                .UseServiceProviderFactory(new ActorApplicationProviderFactory(b => b.ConfigureAutoFac(cb =>
                                                                                                       {
                                                                                                           cb.RegisterInstance(RestartHelper).As<IRestartHelper>();
                                                                                                           cb.RegisterInstance(AppIpManager).As<IAppIpManager>();
                                                                                                       })))
                .ConfigureWebHostDefaults(webBuilder =>
                                          {
                                              #if RELSEASE
                                                   if (AppIpManager.Ip.IsValid)
                                                       webBuilder.UseUrls("http://localhost:5000", $"http://{AppIpManager.Ip.Ip}:5000");
                                                    else 
                                                        webBuilder.UseUrls("http://localhost:5000");
                                              #endif
                                              webBuilder.UseStartup<Startup>();
                                          });
    }
}
