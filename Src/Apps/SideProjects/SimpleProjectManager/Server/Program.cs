using SimpleProjectManager.Server;
using SimpleProjectManager.Server.Configuration;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.DeviceManager;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Server.Core.Tasks;
using Stl.IO;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.KillSwitch;

try
{
    StartConfigManager.ConfigManager.Init(FilePath.Empty);

    IHostBuilder builder = AppNode.StartNode(args, KillRecpientType.Seed, IpcApplicationType.NoIpc, consoleLog: false)
       .RegisterModules(
                new CommonModule(),
                new DataModule(),
                new MainModule(),
                new ProjectionModule(),
                new ServicesModule(),
                new TaskModule(),
                new DeviceModule())
       .ConfigureWebHostDefaults(
            b =>
            {
                StartConfigManager.ConfigManager.ConfigurateWeb(b);
                b.UseStartup<Startup>();
            });

    await builder.Build().RunAsync().ConfigureAwait(false);
}
catch (Exception e)
{
    Console.WriteLine("Schwerer Fehler");
    Console.WriteLine(e);

    Console.ReadKey();
}