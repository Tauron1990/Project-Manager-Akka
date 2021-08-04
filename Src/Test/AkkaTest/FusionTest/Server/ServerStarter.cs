﻿using Akka.Actor;
using AkkaTest.FusionTest.Helper;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Tauron.Application.AkkaNode.Bootstrap;

namespace AkkaTest.FusionTest.Server
{
    public sealed class ServerStarter : IStartUpAction
    {
        private readonly ActorSystem _system;
        private readonly IHostApplicationLifetime _lifetime;

        public ServerStarter(ActorSystem system, IHostApplicationLifetime lifetime)
        {
            _system = system;
            _lifetime = lifetime;
        }
        
        public async void Run()
        {
            if (await ConnectionHelper.Connect(_system, _lifetime))
            {
                AnsiConsole.Clear();
                AnsiConsole.Render(new Rule("Server Gestartet"));
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine("Strg-c zum Beenden");
            }
        }
    }
}