﻿using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using AkkaTest.FusionTest.Helper;
using AkkaTest.FusionTest.Seed;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace AkkaTest.FusionTest.Server
{
    public sealed class ServerStarter : IStartUpAction
    {
        private readonly ActorSystem              _system;
        private readonly IHostApplicationLifetime _lifetime;

        public ServerStarter(ActorSystem system, IHostApplicationLifetime lifetime)
        {
            _system       = system;
            _lifetime     = lifetime;
        }
        
        public async void Run()
        {
            if (await ConnectionHelper.Connect(_system, _lifetime))
            {
                await Task.Delay(1000);
                Console.Title = "Fusion Test Server";
                AnsiConsole.Clear();
                AnsiConsole.Render(new Rule("Server Gestartet"));
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine("Strg-c zum Beenden");
                DistributedPubSub.Get(_system).Mediator.Tell(new Publish(MessageChannel.ChannelId, "Server wurde gestarted"));
            }
        }
    }
}