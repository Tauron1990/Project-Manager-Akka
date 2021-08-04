using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stl.Fusion;
using Stl.Fusion.AkkaBridge;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;

namespace AkkaTest.FusionTest.Server
{
    public static class ServerSetup
    {
        public static IHostBuilder Run(IHostBuilder hostBuilder)
            => hostBuilder.ConfigureLogging(p => p.ClearProviders())
                          .ConfigureServices(
                               c =>
                               {
                                   c.AddScoped<IStartUpAction, ServerStarter>();
                                   c.AddFusion()
                                    .AddAkkaFusionServer();
                               })
                          .StartNode(KillRecpientType.Service, IpcApplicationType.NoIpc);
    }
}