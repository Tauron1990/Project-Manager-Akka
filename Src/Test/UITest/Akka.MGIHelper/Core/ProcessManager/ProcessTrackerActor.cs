using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using Tauron;
using Tauron.Features;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class ProcessTrackerActor : ActorFeatureBase<ProcessTrackerActor.ProcessTrackerState>
    {
        private ProcessTrackerActor() { }

        public static IPreparedFeature New()
            => Feature.Create(() => new ProcessTrackerActor(), c => new ProcessTrackerState(new Timer(_ => c.Self.Tell(GatherProcess.Inst), null, 2000, 2000), ImmutableArray<string>.Empty));

        protected override void ConfigImpl()
        {
            Stop.Subscribe(_ => CurrentState.ProcessTimer.Dispose());

            Receive<ProcessExitMessage>(Handler);

            Receive<RegisterProcessFile>(Handler);

            Receive<GatherProcess>(Handler);

            Receive<Process>(Handler);

            SupervisorStrategy = new OneForOneStrategy(_ => Directive.Stop);
        }

        private IDisposable Handler(IObservable<StatePair<Process, ProcessTrackerState>> obs)
        {
            return obs.Select(
                    p =>
                    {
                        var ok = false;
                        var (process, state) = p;
                        var id = -1;
                        var name = string.Empty;

                        try
                        {
                            name = process.ProcessName;
                            id = process.Id;

                            if (Context.Child(FormatName(process.Id)).Equals(ActorRefs.Nobody)) ok = true;

                            var processName = process.ProcessName;
                            if (ok && state.Tracked.Any(s => s.Contains(processName)))
                                ok = true;
                            else
                                ok = false;

                            if (!ok) process.Dispose();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error While inspecting Process");
                        }

                        return (ok, process, id, name);
                    })
               .Where(p => p.ok)
               .Do(e => Log.Info("Process Found {Name}", e.name))
               .Do(e => Context.ActorOf(FormatName(e.id), TrackedProcessActor.New(e.process, e.id, e.name)))
               .Select(p => new ProcessStateChange(ProcessChange.Started, p.name, p.id, p.process))
               .ToParent();
        }

        private IDisposable Handler(IObservable<StatePair<GatherProcess, ProcessTrackerState>> obs)
        {
            return obs.Where(pair => Context.GetChildren().Count() != pair.State.Tracked.Length)
               .Do(_ => Log.Info("Update Processes"))
               .SelectMany(
                    _ =>
                    {
                        try
                        {
                            return Process.GetProcesses();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error While Recieving Processes");

                            return Array.Empty<Process>();
                        }
                    })
               .ToSelf();
        }

        private IObservable<ProcessTrackerState> Handler(IObservable<StatePair<RegisterProcessFile, ProcessTrackerState>> obs)
        {
            return obs.Where(p => !string.IsNullOrWhiteSpace(p.Event.FileName))
               .Select(p => p.State with { Tracked = p.State.Tracked.Add(p.Event.FileName.Trim()) });
        }

        private IDisposable Handler(IObservable<StatePair<ProcessExitMessage, ProcessTrackerState>> obs)
        {
            return obs.Select(p => p.Event)
               .Select(obj => new ProcessStateChange(ProcessChange.Stopped, obj.Name, obj.Id, obj.Target))
               .ForwardToParent();
        }

        private static string FormatName(int id) => $"Process-{id}";

        public sealed record ProcessTrackerState(Timer ProcessTimer, ImmutableArray<string> Tracked);
    }
}