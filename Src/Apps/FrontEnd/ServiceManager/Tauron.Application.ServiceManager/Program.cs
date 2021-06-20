using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Tauron.Application.AspIntegration;

namespace Tauron.Application.ServiceManager
{
    public class Program
    {
        public static void Main(string[] args) 
            => CreateHostBuilder(args).Build().Run();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                     .UseServiceProviderFactory(new ActorApplicationProviderFactory())
                     .ConfigureWebHostDefaults(webBuilder =>
                                               {
                                                   webBuilder.UseStartup<Startup>();
                                               });
    }
}
