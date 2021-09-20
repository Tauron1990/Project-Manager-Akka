using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using Tauron;
using Tauron.Features;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class TrackedProcessActor : ActorFeatureBase<TrackedProcessActor.TrackedProcessState>
    {
        public sealed record TrackedProcessState(Timer ExitCheck, int Id, string ProcessName, Process Target);

        public static IPreparedFeature New(Process toTrack, int id, string name)
            => Feature.Create(() => new TrackedProcessActor(), c => new TrackedProcessState(new Timer(_ => c.Self.Tell(InternalCheckProcess.Inst), null, 3000, 1000), id, name, toTrack));

        private TrackedProcessActor() { }

        protected override  void ConfigImpl()
        {
            Stop.Subscribe(_ =>
                           {
                               CurrentState.ExitCheck.Dispose();
                               CurrentState.Target.Dispose();
                           });
            
            Receive<InternalCheckProcess>(obs => obs.Where(e => e.State.Target.HasExited)
                                                    .Select(_ => InternalProcessExit.Inst)
                                                    .ToSelf());

            Receive<InternalProcessExit>(obs => obs.Do(p => Log.Info("Track Process {Name} Exited: {Id}", p.State.ProcessName, p.State.Id))
                                                   .Do(_ => Context.Stop(Self))
                                                   .Select(p => new ProcessExitMessage(p.State.Target, p.State.ProcessName, p.State.Id))
                                                   .ToParent());
        }

        private sealed record InternalProcessExit
        {
            internal static readonly InternalProcessExit Inst = new();
        }

        private sealed record InternalCheckProcess
        {
            internal static readonly InternalCheckProcess Inst = new();
        }
    }
}