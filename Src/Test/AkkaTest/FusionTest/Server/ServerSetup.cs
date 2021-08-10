using AkkaTest.FusionTest.Data;
using AkkaTest.FusionTest.Data.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stl.CommandR;
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
                                      .AddComputeService<IClaimManager, ClaimManager>()
                                    .AddServicePublischer(pc => pc.PublishService<IClaimManager>())
                                      .AddAkkaFusionServer();
                               })
                          .StartNode(KillRecpientType.Service, IpcApplicationType.NoIpc,
                               b => b.ConfigureAutoFac(c => c.AddAkkaBridge()));
    }
}