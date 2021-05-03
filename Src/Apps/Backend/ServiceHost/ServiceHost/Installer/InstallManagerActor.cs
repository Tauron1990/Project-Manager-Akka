﻿using System;
using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer.Impl;
using ServiceHost.Services;
using Tauron;
using Tauron.Features;

namespace ServiceHost.Installer
{
    [UsedImplicitly]
    public sealed class InstallManagerActor : ActorFeatureBase<InstallManagerActor.InstallManagerState>
    {
        public sealed record InstallManagerState(IAppRegistry Registry, IConfiguration Configuration, IAppManager AppManager, IAutoUpdater AutoUpdater);

        public static Func<IAppRegistry, IConfiguration, IAppManager, IAutoUpdater, IEnumerable<IPreparedFeature>> New()
        {
            static IEnumerable<IPreparedFeature> _(IAppRegistry registry, IConfiguration configuration, IAppManager manager, IAutoUpdater updater)
            {
                yield return SubscribeFeature.New();
                yield return Feature.Create(() => new InstallManagerActor(), new InstallManagerState(registry, configuration, manager, updater));
            }

            return _;
        }

        protected override void ConfigImpl()
        {
            Receive<InstallerationCompled>(obs => obs.ToUnit(m => TellSelf(SendEvent.Create(m.Event))));
            Receive<InstallRequest>(obs => obs.ForwardToActor(
                                        p => Context.ActorOf(Props.Create<ActualInstallerActor>(p.State.Registry, p.State.Configuration, p.State.AutoUpdater)),
                                        p => p.Event));
            Receive<UninstallRequest>(obs => obs.ForwardToActor(
                                          p => Context.ActorOf(Props.Create<ActualUninstallationActor>(p.State.Registry, p.State.AppManager)),
                                          p => p.Event));
        }
    }
}