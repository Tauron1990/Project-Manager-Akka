using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using Tauron;
using Tauron.Features;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class ProcessManagerActor : ActorFeatureBase<ProcessManagerActor.ProcesManagerState>
    {
        private ProcessManagerActor() { }

        public static IPreparedFeature New()
            => Feature.Create(() => new ProcessManagerActor(), c => new ProcesManagerState(c.ActorOf(ProcessTrackerActor.New()), ImmutableDictionary<string, IActorRef>.Empty));

        protected override void ConfigImpl()
        {
            Receive<RegisterProcessList>(
                obs => obs.Select(
                    p =>
                    {
                        var (script, (processTracker, targetProcesses)) = p;

                        targetProcesses = script.Files
                           .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
                           .Aggregate(
                                targetProcesses,
                                (dic, file) =>
                                {
                                    if (dic.ContainsKey(file))
                                        Log.Error("Only One Script per File Suporrtet: {Script}", script.Intrest.Path.ToString());

                                    processTracker.Tell(new RegisterProcessFile(file));

                                    return dic.SetItem(file, script.Intrest);
                                });

                        return p.State with { TargetProcesses = targetProcesses };
                    }));


            Receive<ProcessStateChange>(
                obs => obs.Select(p => (Message: p.Event, Target: p.State.TargetProcesses.FirstOrDefault(d => d.Key.Contains(p.Event.Name))))
                   .Where(d => !string.IsNullOrWhiteSpace(d.Target.Key))
                   .ToActor(m => m.Target.Value, m => m.Message));
        }

        public sealed record ProcesManagerState(IActorRef ProcessTracker, ImmutableDictionary<string, IActorRef> TargetProcesses);
    }
}