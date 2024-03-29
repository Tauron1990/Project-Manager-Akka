﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.KillSwitch;

namespace AkkaTest.FusionTest.Seed
{
    public static class SeedSetup
    {
        public static IHostBuilder Run(IHostBuilder builder)
            => builder
              .ConfigureServices(c => c.AddTransient<IStartUpAction, SeedStarter>())
               .StartNode(KillRecpientType.Seed, IpcApplicationType.NoIpc);
    }
}