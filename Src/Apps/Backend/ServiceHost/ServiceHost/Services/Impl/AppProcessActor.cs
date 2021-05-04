using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using JetBrains.Annotations;
using ServiceHost.ApplicationRegistry;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AkkaNode.Bootstrap.Console.IpcMessages;
using Tauron.Features;
using Tauron.ObservableExt;

namespace ServiceHost.Services.Impl
{
    public sealed class AppProcessActor : ActorFeatureBase<AppProcessActor.AppProcessorState>
    {
        public sealed record AppProcessorState(InstalledApp App, string ServiceName, IIpcConnection ServiceCom, Process? Process, bool IsProcessRunning, string ServiceId);

        public IPreparedFeature New(InstalledApp app, IIpcConnection connection)
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
                                   b.When(m => m.Process == null && m.IsProcessRunning, o => o.Do(_ => Self.Tell(new InternalStartApp())));

                                   b.When(m => m.Process is {HasExited: false} && m.IsProcessRunning, o => o);

                                   b.When(m => m.Process is {HasExited: true} && m.IsProcessRunning,
                                       o => o.Do(m => m.Process?.Dispose())
                                             .Do(m => Log.Info("Process killed. Restarting {Name}", m.App.Name))
                                             .Do(_ => Self.Tell(new InternalStartApp()))
                                             .Select(m => m with {Process = null}));
                               }));

            Receive<GetName>(o => o.Select(m => new GetNameResponse(m.State.App.Name, m.State.IsProcessRunning)).ToSender());

            Receive<InternalStopApp>(
                o => o.CatchSafe(
                    m => Observable.Return(m.State),
                    (m, e) => Observable.Return(m.State)));
        }

        private sealed record CheckProcess;

        public sealed record GetName;

        public sealed record GetNameResponse(string Name, bool Running);
    }
    
    //    private void StopApp(InternalStopApp obj)
    //    {
    //        CallSafe(
    //            () =>
    //            {
    //                Log.Info("Stop Apps {Name}", _app.Name);
    //                _isProcesRunning = false;
    //                if (_serviceCom == null)
    //                {
    //                    Log.Warning("None Comunication Pipe Killing {Name}", _app.Name);
    //                    _process?.Kill(true);
    //                }
    //                else
    //                {
    //                    if(_process == null) return;

    //                    Log.Info("Sending KillCommand {Name}", _app.Name);
    //                    var writer = new BinaryWriter(_serviceCom);
    //                    writer.Write("Kill-Node");
    //                    writer.Flush();

    //                    Log.Info("Wait for exit {Name}", _app.Name);

    //                    var watch = Stopwatch.StartNew();

    //                    while (watch.Elapsed < TimeSpan.FromMinutes(1))
    //                    {
    //                        Thread.Sleep(1000);
    //                        if(_process.HasExited) return;
    //                    }

    //                    if (_process.HasExited) return;

    //                    Log.Warning("Process not Exited Killing {Name}", _app.Name);
    //                    _process.Kill(true);
    //                }
    //            },
    //            "Error while Stopping Apps",
    //            e =>
    //            {
    //                _process?.Dispose();
    //                if(!Sender.Equals(Context.Parent))
    //                    Sender.Tell(new StopResponse(_app.Name, e));
    //                Context.Parent.Tell(new StopResponse(_app.Name, e));
    //                Context.Stop(Self);
    //            });
    //    }

    //    private void StartApp(InternalStartApp obj)
    //    {
    //        CallSafe(
    //            () =>
    //            {
    //                if(_isProcesRunning) return;
    //                Log.Info("Start Apps {Name}", _app.Name);
    //                _process = Process.Start(new ProcessStartInfo(Path.Combine(_app.Path, _app.Exe), $"--ComHandle {_serviceComName}")
    //                                         {
    //                                             WorkingDirectory = _app.Path,
    //                                             CreateNoWindow = _app.SuressWindow
    //                                         });
    //                _isProcesRunning = true;
    //            }, "Error while Stratin Service");
    //    }

    //    protected override void PreStart()
    //    {
    //        Timers.StartPeriodicTimer(_serviceComName, new CheckProcess(), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
    //        base.PreStart();
    //    }

    //    protected override void PostStop()
    //    {
    //        CallSafe(
    //            () =>
    //            {
    //                _serviceCom?.Dispose();

    //                _process?.Dispose();
    //            }, "Error ehile Disposing Named pipe");

    //        base.PostStop();
    //    }
    //}
}