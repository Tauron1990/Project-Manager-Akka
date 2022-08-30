using Akka.DependencyInjection;
using SimpleProjectManager.Server;
using SimpleProjectManager.Server.Configuration;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Server.Core.Tasks;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;

try
{
    StartConfigManager.ConfigManager.Init();

    var builder = Bootstrap.StartNode(args, KillRecpientType.Seed, IpcApplicationType.NoIpc, consoleLog: true)
       .ConfigureServices(sc => sc.RegisterModules(
                              new AkkaModule(), 
                              new CommonModule(), 
                              new DataModule(),
                              new MainModule(), 
                              new ProjectionModule(), 
                              new ServicesModule(),
                              new TaskModule()))
       .ConfigureWebHostDefaults(
            b =>
            {
                StartConfigManager.ConfigManager.ConfigurateWeb(b);
                b.UseStartup<Startup>();
            });

    await builder.Build().RunAsync();
}
catch (Exception e)
{
    Console.WriteLine("Schwerer Fehler");
    Console.WriteLine(e);

    Console.ReadKey();
}
