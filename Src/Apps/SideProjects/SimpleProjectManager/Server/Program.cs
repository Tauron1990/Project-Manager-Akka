using Akka.Configuration;
using SimpleProjectManager.Server;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;

try
{
    string ip;

    if (File.Exists("seed.conf"))
    {
        var config = ConfigurationFactory.ParseString(File.ReadAllText("seed.conf"));
        ip = config.GetString("akka.remote.dot-netty.tcp.hostname");

        //await SetupRunner.Run(ip);
    }
    else
    #pragma warning disable EX006 // Do not write logic driven by exceptions.
        throw new InvalidOperationException("The File seed.conf does not Exist");
    #pragma warning restore EX006 // Do not write logic driven by exceptions.

    var builder = Bootstrap.StartNode(args, KillRecpientType.Seed, IpcApplicationType.NoIpc, consoleLog: true)
       .ConfigureWebHostDefaults(
            b =>
            {
                b.UseUrls("http://localhost:5000", $"http://{ip}:5000");

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
