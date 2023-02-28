using System.Diagnostics;
using System.Reactive.Linq;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Features;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed class TrackedProcessActor : ActorFeatureBase<TrackedProcessActor.TrackedProcessState>
    {
        private TrackedProcessActor() { }

        public static IPreparedFeature New(Process toTrack, int id, string name)
            => Feature.Create(() => new TrackedProcessActor(), c => new TrackedProcessState(new Timer(_ => c.Self.Tell(InternalCheckProcess.Inst), state: null, 3000, 1000), id, name, toTrack));

        protected override void ConfigImpl()
        {
            Stop.Subscribe(
                _ =>
                {
                    CurrentState.ExitCheck.Dispose();
                    CurrentState.Target.Dispose();
                });

            Observ<InternalCheckProcess>(
                obs => obs.Where(e => e.State.Target.HasExited)
                   .Select(_ => InternalProcessExit.Inst)
                   .ToSelf());

            Observ<InternalProcessExit>(
                obs => obs.Do(p => Logger.LogInformation("Track Process {Name} Exited: {Id}", p.State.ProcessName, p.State.Id))
                   .Do(_ => Context.Stop(Self))
                   .Select(p => new ProcessExitMessage(p.State.Target, p.State.ProcessName, p.State.Id))
                   .ToParent());
        }

        public sealed record TrackedProcessState(Timer ExitCheck, int Id, string ProcessName, Process Target);

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