using System.Collections.Immutable;
using System.Diagnostics;
using System.Reactive.Linq;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Features;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed class ProcessTrackerActor : ActorFeatureBase<ProcessTrackerActor.ProcessTrackerState>
    {
        private ProcessTrackerActor() { }

        public static IPreparedFeature New()
            => Feature.Create(
                () => new ProcessTrackerActor(),
                _ => new ProcessTrackerState(Gartherer: null, ImmutableArray<string>.Empty));

        protected override void ConfigImpl()
        {
            Stop.Subscribe(_ => CurrentState.Gartherer?.Dispose());

            Observ<ProcessExitMessage>(Handler);

            Observ<StartProcessTracking>(Handler);

            Observ<Process>(Handler);

            SupervisorStrategy = new OneForOneStrategy(_ => Directive.Stop);
        }

        private IDisposable Handler(IObservable<StatePair<Process, ProcessTrackerState>> obs)
        {
            return obs.Select(
                    p =>
                    {
                        var ok = false;
                        (Process process, ProcessTrackerState state) = p;
                        int id = -1;
                        var name = string.Empty;

                        try
                        {
                            name = process.ProcessName;
                            id = process.Id;

                            if (Context.Child(FormatName(process.Id)).Equals(ActorRefs.Nobody))
                                ok = true;

                            string processName = process.ProcessName;
                            ok = ok && state.Tracked.Any(s => s.Contains(processName, StringComparison.Ordinal));
                            

                            if (!ok) 
                                process.Dispose();
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(e, "Error While inspecting Process");
                        }

                        return (ok, process, id, name);
                    })
               .Where(p => p.ok)
               .Do(e => Logger.LogInformation("Process Found {Name}", e.name))
               .Do(e => Context.ActorOf(FormatName(e.id), TrackedProcessActor.New(e.process, e.id, e.name)))
               .Select(p => new ProcessStateChange(ProcessChange.Started, p.name, p.id, p.process))
               .ToParent();
        }


        private IObservable<ProcessTrackerState> Handler(IObservable<StatePair<StartProcessTracking, ProcessTrackerState>> obs)
        {
            return obs.Select(p => p.State with
                                   {
                                       Tracked = p.Event.FileNames
                                          .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim())
                                          .ToImmutableArray(),
                                       Gartherer = new ProcessGartherer(p.Self, Logger),
                                   })
               .Do(_ => Process.GetProcesses().Foreach(p => Self.Tell(p)),
                    ex => Logger.LogCritical(ex, "Error on Start process tracking"));
        }

        private IDisposable Handler(IObservable<StatePair<ProcessExitMessage, ProcessTrackerState>> obs)
        {
            return obs.Select(p => p.Event)
               .Select(obj => new ProcessStateChange(ProcessChange.Stopped, obj.Name, obj.Id, obj.Target))
               .ForwardToParent();
        }

        private static string FormatName(int id) => $"{nameof(Process)}-{id}";

        public sealed record ProcessTrackerState(ProcessGartherer? Gartherer, ImmutableArray<string> Tracked);
    }
}