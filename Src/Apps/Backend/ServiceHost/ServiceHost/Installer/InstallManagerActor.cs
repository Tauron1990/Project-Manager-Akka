using System;
using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer.Impl;
using ServiceHost.Services;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;

namespace ServiceHost.Installer
{
    [UsedImplicitly]
    public sealed class InstallManagerActor : ActorFeatureBase<InstallManagerActor.InstallManagerState>
    {
        public static Func<IAppRegistry, AppNodeInfo, IAppManager, IAutoUpdater, IEnumerable<IPreparedFeature>> New()
        {
            static IEnumerable<IPreparedFeature> _(IAppRegistry registry, AppNodeInfo configuration, IAppManager manager, IAutoUpdater updater)
            {
                yield return SubscribeFeature.New();
                yield return Feature.Create(() => new InstallManagerActor(), new InstallManagerState(registry, configuration, manager, updater));
            }

            return _;
        }

        protected override void ConfigImpl()
        {
            Receive<SubscribeInstallationCompled>(
                obs => obs.ToUnit(
                    r =>
                    {
                        if (r.Event.Unsubscribe)
                            Self.Forward(new EventUnSubscribe(typeof(InstallerationCompled)));
                        else
                            Self.Forward(new EventSubscribe(Watch: true, typeof(InstallerationCompled)));
                        Sender.Tell(new SubscribeInstallationCompledResponse(new EventSubscribtion(typeof(InstallerationCompled), Self), Success: true));
                    }));

            Receive<InstallerationCompled>(obs => obs.ToUnit(m => TellSelf(SendEvent.Create(m.Event))));

            Receive<InstallRequest>(
                obs => obs.ForwardToActor(
                    p => Context.ActorOf(Props.Create<ActualInstallerActor>(p.State.Registry, p.State.Configuration, p.State.AutoUpdater)),
                    p => p.Event));
            Receive<UninstallRequest>(
                obs => obs.ForwardToActor(
                    p => Context.ActorOf(Props.Create<ActualUninstallationActor>(p.State.Registry, p.State.AppManager)),
                    p => p.Event));
        }

        public sealed record InstallManagerState(IAppRegistry Registry, AppNodeInfo Configuration, IAppManager AppManager, IAutoUpdater AutoUpdater);
    }
}