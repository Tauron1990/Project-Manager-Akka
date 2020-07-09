﻿using System;
using System.Threading.Tasks;
using Akka.Cluster;
using Tauron.Application.AkkaNode.Boottrap;
using Tauron.Application.Master.Commands;
using Tauron.Host;

namespace ServiceHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ActorApplication.Create(args)
                .StartNode(KillRecpientType.Host)
                .ConfigurateAkkaSystem((context, system) =>
                {
                    var cluster = Cluster.Get(system);
                    cluster.RegisterOnMemberUp(()
                        => ServiceRegistry.GetRegistry(system).RegisterService(new RegisterService(context.HostEnvironment.ApplicationName, cluster.SelfUniqueAddress)));
                })
                .Build().Run();

        }
    }
}
