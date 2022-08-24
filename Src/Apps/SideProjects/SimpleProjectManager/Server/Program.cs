using SimpleProjectManager.Server;
using SimpleProjectManager.Server.Configuration;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;

try
{
    StartConfigManager.ConfigManager.Init();

    var builder = Bootstrap.StartNode(args, KillRecpientType.Seed, IpcApplicationType.NoIpc, consoleLog: true)
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
