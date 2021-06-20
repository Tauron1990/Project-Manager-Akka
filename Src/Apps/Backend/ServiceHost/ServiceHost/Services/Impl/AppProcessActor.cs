using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using ServiceHost.ApplicationRegistry;
using Tauron;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Bootstrap.Console.IpcMessages;
using Tauron.Features;
using Tauron.ObservableExt;

namespace ServiceHost.Services.Impl
{
    public sealed class AppProcessActor : ActorFeatureBase<AppProcessActor.AppProcessorState>
    {
        public sealed record AppProcessorState(InstalledApp App, string ServiceName, IIpcConnection ServiceCom, Process? Process, bool IsProcessRunning, string ServiceId);

        public static IPreparedFeature New(InstalledApp app, IIpcConnection connection)
            => Feature.Create(() => new AppProcessActor(), _ => new AppProcessorState(app, Guid.NewGuid().ToString("N"), connection, null, false, string.Empty));

        protected override void ConfigImpl()
        {
            var dispo = CurrentState.ServiceCom.OnMessage<RegisterNewClient>()
                                    .OnResult()
                                    .Where(m => m.ServiceName == CurrentState.ServiceName)
                                    .SubscribeWithStatus(TellSelf);

            Receive<RegisterNewClient>(
                obs => from client in obs 
                       select client.State with{ServiceId = client.Event.Id});

            Stop.SubscribeWithStatus(_ =>
                                     {
                                         dispo.Dispose();
                                         CurrentState.Process?.Dispose();
                                     });

            Receive<CheckProcess>(
                obs => obs
                      .Select(m => m.State)
                      .ConditionalSelect()
                          .ToResult<AppProcessorState>(
                               b =>
                               {
                                   b.When(m => m.Process == null && m.IsProcessRunning, o => o.Do(_ => Self.Tell(new InternalStartApp()))
                                                                                              .Select(s => s with{IsProcessRunning = false}));

                                   b.When(m => m.Process is {HasExited: false} && m.IsProcessRunning, o => o);

                                   b.When(m => m.Process is {HasExited: true} && m.IsProcessRunning,
                                       o => o.Do(m => m.Process?.Dispose())
                                             .Do(m => Log.Info("Process killed. Restarting {Name}", m.App.Name))
                                             .Do(_ => Self.Tell(new InternalStartApp()))
                                             .Select(m => m with {Process = null, IsProcessRunning = false}));
                               }));

            Receive<GetName>(o => o.Select(m => new GetNameResponse(m.State.App.Name, m.State.IsProcessRunning)).ToSender());

            Receive<InternalStopApp>(
                obs => obs
                      .Where(m => m.State.IsProcessRunning)
                      .CatchSafe(
                           m => Observable.Return(m.State with {IsProcessRunning = m.Event.Restart})
                                          .ConditionalSelect()
                                          .ToSame(
                                               b =>
                                               {
                                                   b.When(s => string.IsNullOrWhiteSpace(s.ServiceId), o => o.Do(s => Log.Warning("None Comunication Client Registrated {Name}", s.App.Name))
                                                                                                             .Do(s => s.Process?.Kill(true))
                                                                                                             .Do(s => s.Process?.Dispose())
                                                                                                             .Select(s => s with {Process = null}));

                                                   b.When(s => !string.IsNullOrWhiteSpace(s.ServiceId) && s.Process == null, o => o);

                                                   b.When(s => !string.IsNullOrWhiteSpace(s.ServiceId) && s.Process != null,
                                                       o => o.Do(s => Log.Info("Sending Kill Command to App {Name}", s.App.Name))
                                                             .Do(s => s.ServiceCom.SendMessage(s.ServiceId, new KillNode()))
                                                             .Do(s => Log.Info("Wait for exit {Name}", s.App.Name))
                                                             .Select(s =>
                                                                     {
                                                                         AppProcessorState NewState()
                                                                             => s with {Process = null};

                                                                         var watch = Stopwatch.StartNew();
                                                                         using var prc = s.Process;
                                                                         if (prc == null) return s;

                                                                         while (watch.Elapsed < TimeSpan.FromMinutes(1))
                                                                         {
                                                                             Thread.Sleep(1000);
                                                                             if (prc.HasExited) return NewState();
                                                                         }

                                                                         if (prc.HasExited) return NewState();

                                                                         Log.Warning("Process not Exited Killing {Name}", s.App.Name);
                                                                         prc.Kill(true);

                                                                         return NewState();
                                                                     })
                                                             .Do(s => s.Process?.Kill(true))
                                                             .Do(s => s.Process?.Dispose()));
                                               }),
                           (m, e) => Observable.Return(m.State)
                                               .Do(s => Log.Error(e, "Error while Stopping App {Name}", s.App.Name))
                                               .Do(s => s.Process?.Dispose())
                                               .ApplyWhen(_ => !Sender.Equals(Parent), s => Sender.Tell(new StopResponse(s.App.Name, true)))
                                               .Do(s => Parent.Tell(new StopResponse(s.App.Name, true)))
                                               .Do(_ => Context.Stop(Self))
                                               .Select(s => s with {Process = null})));

            Receive<InternalStartApp>(
                obs => obs
                      .Where(m => !m.State.IsProcessRunning)
                      .CatchSafe(
                           p => from state in Observable.Return(p.State)
                                                        .Do(s => Log.Info("Start App {Name}", s.App.Name))
                                let process = Process.Start(new ProcessStartInfo(Path.Combine(state.App.Path, state.App.Exe), $"--ComHandle {state.ServiceId}")
                                                            {
                                                                WorkingDirectory = state.App.Path
                                                            })
                                select state with {IsProcessRunning = true, Process = process},
                           (m, e) => Observable.Return(m.State)
                                               .Do(s => Log.Error(e, "Error while Starting App {Name}", s.App.Name))));

            Timers.StartPeriodicTimer(CurrentState.ServiceName, new CheckProcess(), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        }

        private sealed record CheckProcess;

        public sealed record GetName;

        public sealed record GetNameResponse(string Name, bool Running);
    }
}