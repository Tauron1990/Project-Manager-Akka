
using Akka.Configuration;
using SimpleProjectManager.Server;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;

var builder = Bootstrap.StartNode(args, KillRecpientType.Seed, IpcApplicationType.NoIpc, consoleLog: true)
   .ConfigureWebHostDefaults(
        b =>
        {
            if (File.Exists("seed.conf"))
            {
                var config = ConfigurationFactory.ParseString(File.ReadAllText("seed.conf"));
                var ip = config.GetString("akka.remote.dot-netty.tcp.hostname");
                b.UseUrls("http://localhost:85", $"http://{ip}:85");
            }
            else
                b.UseUrls("http://localhost:85");//, "http://192.168.105.96:85");
            b.UseStartup<Startup>();
        });

builder.Build().Run();
