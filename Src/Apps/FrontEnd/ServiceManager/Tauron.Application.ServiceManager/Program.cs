using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Tauron.Application.AspIntegration;
using Tauron.Application.ServiceManager.AppCore;
using Tauron.Application.ServiceManager.AppCore.Helper;

namespace Tauron.Application.ServiceManager
{
    public class Program
    {
        private static readonly RestartHelper RestartHelper = new();
        private static readonly AppIpManager AppIpManager = new();

        public static async Task Main(string[] args)
        {
            await AppIpManager.Aquire();
            await CreateHostBuilder(args).Build().RunAsync();

            if (!RestartHelper.Restart) return;
            
            var file = Path.ChangeExtension(Assembly.GetEntryAssembly()?.Location, ".exe");
            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                Process.Start(file);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
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