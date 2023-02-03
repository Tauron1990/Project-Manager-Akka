using System;
using AkkaTest.FusionTest.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stl.Fusion;
using Stl.Fusion.AkkaBridge;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Logging;
using Tauron.Application.Master.Commands.KillSwitch;

namespace AkkaTest.FusionTest.Client
{
    public static class ClientSetup
    {
        public static IHostBuilder Run(IHostBuilder builder)
        {
            return builder
                  .ConfigureLogging(b => b.AddNLog(setupBuilder => setupBuilder.ConfigurateFile($"Akka Test Client {Guid.NewGuid():N}")))
                  .ConfigureServices(
                       s =>
                       {
                           s.AddTransient<IStartUpAction, ClientSetupStarter>();
                           s.AddFusion()
                            .AddAkkaFusionClient(b => b.AddReplicaService<IClaimManager>());
                       })
                  .StartNode(
                       KillRecpientType.Service,
                       IpcApplicationType.NoIpc,
                       b => b.ConfigureAutoFac(b => b.AddAkkaBridge()));
        }
    }
}