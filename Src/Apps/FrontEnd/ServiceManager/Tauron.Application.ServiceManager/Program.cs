using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        private static readonly RestartHelper _restartHelper = new();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            if (!_restartHelper.Restart) return;
            
            var file = Path.ChangeExtension(Assembly.GetEntryAssembly()?.Location, ".exe");
            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                Process.Start(file);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                     .UseServiceProviderFactory(new ActorApplicationProviderFactory(b => b.ConfigureAutoFac(cb => cb.RegisterInstance(_restartHelper).As<IRestartHelper>())))
                     .ConfigureWebHostDefaults(webBuilder =>
                                               {
                                                   webBuilder.UseStartup<Startup>();
                                               });
    }
}
