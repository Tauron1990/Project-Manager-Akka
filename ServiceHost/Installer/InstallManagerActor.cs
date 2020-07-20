﻿using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer.Impl;
using ServiceHost.Services;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;

namespace ServiceHost.Installer
{
    [UsedImplicitly]
    public sealed class InstallManagerActor : ExposedReceiveActor
    {
        public InstallManagerActor(IAppRegistry registry, IConfiguration configuration, IAppManager manager)
        {
            SubscribeAbility ability = new SubscribeAbility(this);
            ability.MakeReceive();

            Receive<InstallerationCompled>(ic => ability.Send(ic));
            Receive<InstallRequest>(o => Context.ActorOf(Props.Create<ActualInstallerActor>(registry, configuration)).Forward(o));
            Receive<UninstallRequest>(o => Context.ActorOf(Props.Create<ActualUninstallationActor>(registry, manager, configuration)).Forward(o));
        }
    }
}