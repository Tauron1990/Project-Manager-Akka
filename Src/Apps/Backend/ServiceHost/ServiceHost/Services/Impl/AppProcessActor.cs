using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using ServiceHost.ApplicationRegistry;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.IpcMessages;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;
using Tauron.ObservableExt;
using Tauron.Servicemnager.Networking;
using Tauron.Servicemnager.Networking.IPC;

namespace ServiceHost.Services.Impl
{
    public sealed class AppProcessActor : ActorFeatureBase<AppProcessActor.AppProcessorState>
    {
        public static IPreparedFeature New(InstalledApp app, IIpcConnection connection)
            => Feature.Create(
                () => new AppProcessActor(), 
                _ => new AppProcessorState(app, Guid.NewGuid().ToString("N"), connection, null, ProcessState: AppState.None, SharmProcessId.Empty));

        protected override void ConfigImpl()
        {
            var dispo = CurrentState.ServiceCom.OnMessage<RegisterNewClient>()
               .OnResult()
               .Where(m => m.ServiceName == CurrentState.ServiceName)
               .SubscribeWithStatus(TellSelf);

            Receive<RegisterNewClient>(
                obs => from client in obs
                       select client.State with { ServiceId = client.Event.Id });

            Stop.SubscribeWithStatus(
                _ =>
                {
                    dispo.Dispose();
                    CurrentState.Process?.Dispose();
                });

            Receive<CheckProcess>(
                obs => obs
                   .Select(m => m.State)
                   .Do(s => s.Process?.Refresh())
                   .ConditionalSelect()
                   .ToResult<AppProcessorState>(
                        b =>
                        {
                            b.When(
                                m => m.Process == null && m.ProcessState == AppState.Running,
                                o => o.Do(_ => Self.Tell(new InternalStartApp()))
                                   .Select(s => s with { ProcessState = AppState.NotRunning }));

                            b.When(m => m is { Process: { HasExited: false }, ProcessState: AppState.Running }, o => o);

                            b.When(
                                m => m is { Process: { HasExited: true }, ProcessState: AppState.Running },
                                o => o.Do(m => m.Process?.Dispose())
                                   .Do(m => Logger.LogInformation("Process killed. Restarting {Name}", m.App.Name))
                                   .Do(_ => Self.Tell(new InternalStartApp()))
                                   .Select(m => m with { Process = null, ProcessState = AppState.NotRunning }));
                        }));

            Receive<GetName>(o => o.Select(m => new GetNameResponse(m.State.App.Name, m.State.ProcessState)).ToSender());

            Receive<InternalStopApp>(
                obs => obs
                   .Where(m => m.State.ProcessState == AppState.Running)
                   .CatchSafe(
                        m => Observable.Return(m.State with { ProcessState = m.Event.Restart })
                           .ConditionalSelect()
                           .ToSame(
                                b =>
                                {
                                    b.When(
                                        s => s.ServiceId.IsEmpty,
                                        o => o.Do(s => Logger.LogWarning("None Comunication Client Registrated {Name}", s.App.Name))
                                           .Do(s => s.Process?.Kill(entireProcessTree: true))
                                           .Do(s => s.Process?.Dispose())
                                           .Select(s => s with { Process = null }));

                                    b.When(s => !s.ServiceId.IsEmpty && s is { Process: null }, o => o);

                                    b.When(
                                        s => !s.ServiceId.IsEmpty && s.Process != null,
                                        o => o.Do(s => Logger.LogInformation("Sending Kill Command to App {Name}", s.App.Name))
                                           .Do(s => s.ServiceCom.SendMessage(Client.From(s.ServiceId.Value), new KillNode()))
                                           .Do(s => Logger.LogInformation("Wait for exit {Name}", s.App.Name))
                                           .Select(
                                                s =>
                                                {
                                                    AppProcessorState NewState()
                                                        => s with { Process = null };

                                                    var watch = Stopwatch.StartNew();
                                                    using var prc = s.Process;

                                                    if (prc == null) return s;

                                                    while (watch.Elapsed < TimeSpan.FromMinutes(1))
                                                    {
                                                        Thread.Sleep(1000);

                                                        prc.Refresh();
                                                        if (prc.HasExited) return NewState();
                                                    }

                                                    if (prc.HasExited) return NewState();

                                                    Logger.LogWarning("Process not Exited Killing {Name}", s.App.Name);
                                                    prc.Kill(entireProcessTree: true);

                                                    return NewState();
                                                })
                                           .Do(s => s.Process?.Kill(entireProcessTree: true))
                                           .Do(s => s.Process?.Dispose()));
                                }),
                        (m, e) => Observable.Return(m.State)
                           .Do(s => Logger.LogError(e, "Error while Stopping App {Name}", s.App.Name))
                           .Do(s => s.Process?.Dispose())
                           .ApplyWhen(_ => !Sender.Equals(Parent), s => Sender.Tell(new StopResponse(s.App.Name, Error: true)))
                           .Do(s => Parent.Tell(new StopResponse(s.App.Name, Error: true)))
                           .Do(_ => Context.Stop(Self))
                           .Select(s => s with { Process = null })));

            Receive<InternalStartApp>(
                obs => obs
                   .Where(m => m.State.ProcessState != AppState.NotRunning)
                   .CatchSafe(
                        p => from state in Observable.Return(p.State)
                                .Do(s => Logger.LogInformation("Start App {Name}", s.App.Name))
                             let process = Process.Start(
                                 new ProcessStartInfo(Path.Combine(state.App.Path, state.App.Exe), $"--ComHandle {state.ServiceId}")
                                 {
                                     WorkingDirectory = state.App.Path,
                                 })
                             select state with { ProcessState = AppState.Running, Process = process },
                        (m, e) => Observable.Return(m.State)
                           .Do(s => Logger.LogError(e, "Error while Starting App {Name}", s.App.Name))));

            Timers.StartPeriodicTimer(CurrentState.ServiceName, new CheckProcess(), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        }

        public sealed record AppProcessorState(InstalledApp App, string ServiceName, IIpcConnection ServiceCom, Process? Process, AppState ProcessState, SharmProcessId ServiceId);

        private sealed record CheckProcess;

        public sealed record GetName;

        public sealed record GetNameResponse(AppName Name, AppState Running);
    }
}