
using SimpleProjectManager.Server;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;

var builder = Host.CreateDefaultBuilder(args)
   .StartNode(KillRecpientType.Seed, IpcApplicationType.NoIpc, consoleLog: true)
   .ConfigureWebHostDefaults(
        b =>
        {
            b.UseUrls("http://localhost:85");//, "http://192.168.105.96:85");
            b.UseStartup<Startup>();
        });

builder.Build().Run();
