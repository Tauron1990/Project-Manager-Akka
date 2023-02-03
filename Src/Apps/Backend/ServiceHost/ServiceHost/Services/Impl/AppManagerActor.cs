using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using ServiceHost.ApplicationRegistry;
using ServiceHost.Installer;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;
using Tauron.ObservableExt;

namespace ServiceHost.Services.Impl
{
    public sealed class AppManagerActor : ActorFeatureBase<AppManagerActor.AppManagerState>
    {
        public static Func<IAppRegistry, IInstaller, InstallChecker, IIpcConnection, IEnumerable<IPreparedFeature>> New()
        {
            static IEnumerable<IPreparedFeature> _(IAppRegistry appRegistry, IInstaller installer, InstallChecker installChecker, IIpcConnection ipc)
            {
                yield return SubscribeFeature.New();
                yield return Feature.Create(() => new AppManagerActor(), new AppManagerState(appRegistry, installer, installChecker, ipc));
            }

            return _;
        }

        protected override void ConfigImpl()
        {
            CurrentState.Installer.Tell(new EventSubscribe(Watch: true, typeof(InstallerationCompled)));

            Receive<UpdateTitle>(o => o.ToUnit(() => Console.Title = "Application Host"));

            Receive<InstallerationCompled>(
                obs =>
                {
                    void PipeToSelf(IAppRegistry appRegistry, string name)
                    {
                        appRegistry.Actor
                           .Ask<InstalledAppRespond>(new InstalledAppQuery(name), TimeSpan.FromSeconds(10))
                           .PipeTo(Self, success: ar => new StartApp(ar.App)).Ignore();
                    }

                    return obs.ConditionalSelect()
                       .ToResult<Unit>(
                            b =>
                            {
                                b.When(m => m.State.InstallChecker.IsInstallationStart, o => o.Do(_ => Context.System.Terminate()).ToUnit());
                                b.When(
                                    m => !m.State.InstallChecker.IsInstallationStart,
                                    o => o.Where(m => m.Event.Succesfull && m.Event.InstallAction == InstallationAction.Install)
                                       .ToUnit(
                                            m =>
                                            {
                                                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                                                switch (m.Event.Type)
                                                {
                                                    case AppType.Cluster:
                                                        Cluster.Get(Context.System)
                                                           .RegisterOnMemberUp(() => PipeToSelf(m.State.AppRegistry, m.Event.Name));

                                                        break;
                                                    case AppType.StartUp:
                                                        PipeToSelf(m.State.AppRegistry, m.Event.Name);

                                                        break;
                                                }
                                            }));
                            });
                });

            Receive<StopApps>(
                obs => obs.SelectMany(_ => Context.GetChildren())
                   .ToActor(a => a, _ => new InternalStopApp()));

            Receive<StopApp>(
                obs => (
                        from data in obs
                        select (Child: Context.Child(data.Event.Name), Sender, data.Event.Name)
                    ).ConditionalSelect()
                   .ToResult<Unit>(
                        b =>
                        {
                            b.When(m => m.Child.IsNobody(), o => o.ToUnit(a => a.Sender.Tell(new StopResponse(a.Name, Error: false))));
                            b.When(m => !m.Child.IsNobody(), o => o.ToUnit(a => a.Child.Forward(new InternalStopApp())));
                        }));

            Receive<StopResponse>(obs => obs.ToUnit(m => TellSelf(SendEvent.Create(m.Event))));

            Receive<StartApps>(
                obs => obs.CatchSafe(
                    r => (from request in Observable.Return(r)
                          from result in request.State.AppRegistry.Ask<AllAppsResponse>(new AllAppsQuery(), TimeSpan.FromMinutes(1))
                          from app in result.Apps
                          select new InternalFilterApp(request.Event.AppType, app)
                        ).UToActor(Self),
                    (_, e) => Observable.Return(Unit.Default)
                       .Do(_ => Log.Error(e, "Error On Begin Start All Apps"))));

            Receive<InternalFilterApp>(
                obs => obs.CatchSafe(
                    f => (from filter in Observable.Return(f)
                          from app in filter.State.AppRegistry.Ask<InstalledAppRespond>(new InstalledAppQuery(filter.Event.Name), TimeSpan.FromMinutes(1))
                          where !app.Fault && app.App.AppType == filter.Event.AppType
                          select new StartApp(app.App)
                        ).UToActor(Self),
                    (_, e) => Observable.Return(Unit.Default)
                       .Do(_ => Log.Error(e, "Erro while Query App Info"))));

            Receive<StartApp>(
                obs => (from request in obs
                        where !request.Event.App.IsEmpty()
                        where Context.Child(request.Event.App.Name).IsNobody()
                        select (Msg: new InternalStartApp(), Actor: Context.ActorOf(AppProcessActor.New(request.Event.App, request.State.Ipc)))
                    ).ToActor(a => a.Actor, m => m.Msg));

            #region SharedApi

            Receive<StopAllApps>(
                obs => obs.Select(p => p.NewEvent(p.Event))
                   .CatchSafe(
                        p => from request in Observable.Return(p)
                             from response in Task.WhenAll(
                                 Context.GetChildren()
                                    .Select(c => c.Ask<StopResponse>(new InternalStopApp(), TimeSpan.FromMinutes(1))))
                             select request.NewEvent(new StopAllAppsResponse(response.All(r => !r.Error))),
                        (r, e) => Observable.Return(r.NewEvent(new StopAllAppsResponse(Success: false)))
                           .Do(_ => Log.Warning(e, "Error on Shared Api Stop All Apps")))
                   .ToActor(a => a.Sender, m => m.Event));

            Receive<StartAllApps>(
                obs => obs.Do(_ => Self.Tell(new StartApps(AppType.Cluster)))
                   .Select(_ => new StartAllAppsResponse(Success: true))
                   .ToSender());

            Receive<QueryAppStaus>(
                obs => obs.CatchSafe(
                        p => from request in Observable.Return(p)
                             from status in Task.WhenAll(
                                 request.Context.GetChildren()
                                    .Select(
                                         c => c.Ask<AppProcessActor.GetNameResponse>(new AppProcessActor.GetName(), TimeSpan.FromSeconds(2))
                                            .ContinueWith(
                                                 t =>
                                                 {
                                                     if (t.IsCompletedSuccessfully)
                                                         return t.Result;

                                                     if (t.IsFaulted)
                                                         Log.Warning(t.Exception.Unwrap(), "Error on Recive Process Apss Name");

                                                     return null;
                                                 })))
                             select request.NewEvent(
                                 new AppStatusResponse(
                                     request.Event.OperationId,
                                     status.Where(e => e != null)
                                        .ToImmutableDictionary(g => g.Name, g => g.Running))),
                        (r, e) => Observable.Return(r.NewEvent(new AppStatusResponse(r.Event.OperationId, ImmutableDictionary<string, bool>.Empty)))
                           .Do(_ => Log.Error(e, "Error getting Status")))
                   .ToActor(a => a.Sender, a => a.Event));

            Receive<StopHostApp>(
                obs => obs.CatchSafe(
                        p => from request in Observable.Return(p)
                             let child = request.Context.Child(request.Event.AppName)
                             from response in child.IsNobody()
                                 ? Task.FromResult(default(StopResponse))
                                 : child.Ask<StopResponse?>(new InternalStopApp(), TimeSpan.FromMinutes(1))
                             select request.NewEvent(new StopHostAppResponse(response is { Error: false })),
                        (r, e) => Observable.Return(r.NewEvent(new StopHostAppResponse(Success: false)))
                           .Do(_ => Log.Warning(e, "Error Shared Api Stop")))
                   .ToActor(a => a.Sender, m => m.Event));

            Receive<StartHostApp>(
                obs => (from request in obs
                        select request.NewEvent((request.Event, Child: request.Context.Child(request.Event.AppName)))
                    ).ConditionalSelect()
                   .ToResult<StatePair<StartHostAppResponse, AppManagerState>>(
                        b =>
                        {
                            b.When(
                                p => !p.Event.Child.IsNobody(),
                                o => o.Do(p => p.Event.Child.Tell(new InternalStartApp()))
                                   .Select(p => p.NewEvent(new StartHostAppResponse(Success: true))));

                            b.When(
                                p => p.Event.Child.IsNobody(),
                                o => o.CatchSafe(
                                    p => (from request in Observable.Return(p)
                                          from app in request.State.AppRegistry.Ask<InstalledAppRespond>(new InstalledAppQuery(request.Event.Event.AppName), TimeSpan.FromMinutes(1))
                                          select app
                                        ).ApplyWhen(m => !m.Fault, m => p.Self.Tell(new StartApp(m.App)))
                                       .Select(m => p.NewEvent(new StartHostAppResponse(!m.Fault))),
                                    (p, e) => Observable.Return(p.NewEvent(new StartHostAppResponse(Success: false)))
                                       .Do(_ => Log.Warning(e, "Error on Shared Api Start"))));
                        })
                   .ToActor(a => a.Sender, m => m.Event));

            Receive<RestartApp>(
                obs => (from request in obs
                        let child = Context.Child(request.Event.Name)
                        where !child.IsNobody()
                        select (child, Event: new InternalStopApp(Restart: true)))
                   .ToUnit(evt => evt.child.Tell(evt.Event)));

            #endregion

            Timers.StartPeriodicTimer(new object(), new UpdateTitle(), TimeSpan.FromSeconds(10));
            CurrentState.AppRegistry.Tell(new EventSubscribe(Watch: true, typeof(RegistrationResponse)));

            CoordinatedShutdown.Get(Context.System)
               .AddTask(CoordinatedShutdown.PhaseBeforeServiceUnbind, "AppManagerShutdown", new ContextShutdown(Log, Context).HostShutdown);
        }

        public sealed record AppManagerState(IAppRegistry AppRegistry, IInstaller Installer, InstallChecker InstallChecker, IIpcConnection Ipc);

        private sealed record InternalFilterApp(AppType AppType, string Name);

        private sealed class ContextShutdown
        {
            private readonly IActorContext _context;
            private readonly ILoggingAdapter _log;

            internal ContextShutdown(ILoggingAdapter log, IActorContext context)
            {
                _log = log;
                _context = context;
            }

            internal Task<Done> HostShutdown()
            {
                _log.Info("Shutdown All Host Apps");

                return Task.WhenAll(
                    _context
                       .GetChildren()
                       .Select(ar => ar.Ask<StopResponse>(new InternalStopApp(), TimeSpan.FromMinutes(2)))
                       .ToArray()).ContinueWith(_ => Done.Instance);
            }
        }

        private sealed record UpdateTitle;
    }
}