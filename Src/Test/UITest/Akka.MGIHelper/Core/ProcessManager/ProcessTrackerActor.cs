using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Akka.Actor;
using Akka.Util;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Features;

namespace Akka.MGIHelper.Core.ProcessManager
{
    public sealed class ProcessTrackerActor : ActorFeatureBase<ProcessTrackerActor.ProcessTrackerState>
    {
        private ProcessTrackerActor() { }

        public static IPreparedFeature New()
            => Feature.Create(
                () => new ProcessTrackerActor(),
                _ => new ProcessTrackerState(Gartherer: null, ImmutableArray<string>.Empty, 0, 0, ImmutableList<string>.Empty));

        protected override void ConfigImpl()
        {
            Stop.Subscribe(_ => CurrentState.Gartherer?.Dispose());

            Receive<ProcessExitMessage>(Handler);

            Receive<StartProcessTracking>(Handler);

            //Receive<GatherProcess>(Handler);

            Receive<Process>(Handler);

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

                            var processName = process.ProcessName;
                            ok = ok && state.Tracked.Any(s => s.Contains(processName, StringComparison.Ordinal));
                            var isPriority = p.State.PriorityProcesses.Any(prio => processName.Contains(prio, StringComparison.Ordinal));
                            
                            ConfigProcess(process, ok || isPriority);

                            if (!ok) process.Dispose();
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

        private void ConfigProcess(Process process, bool isCLient)
        {
            void SetAffinity(int bits)
            {
                if(bits == 0) return;
                #if DEBUG
                // ReSharper disable once RedundantJumpStatement
                return;
                #else
                Try<IntPtr>.From(() => process.ProcessorAffinity = (IntPtr)bits);
                #endif
            }
            
#pragma warning disable GU0017
            (_, _, int clientAffinity, int operationSystemAffinity, _) = CurrentState;
#pragma warning restore GU0017
            
            if (isCLient)
            {
                SetAffinity(clientAffinity);
                Try<ProcessPriorityClass>.From(() => process.PriorityClass = ProcessPriorityClass.RealTime);
            }
            else 
                SetAffinity(operationSystemAffinity);
        }
        
        /*private IDisposable Handler(IObservable<StatePair<GatherProcess, ProcessTrackerState>> obs)
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
        }*/

        private IObservable<ProcessTrackerState> Handler(IObservable<StatePair<StartProcessTracking, ProcessTrackerState>> obs)
        {
            return obs.Select(p => p.State with
                                   {
                                       Tracked = p.Event.FileNames
                                          .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim())
                                          .ToImmutableArray(),
                                       ClientAffinity = p.Event.ClientAffinity,
                                       OperationSystemAffinity = p.Event.OperatingAffinity,
                                       Gartherer = new ProcessGartherer(p.Self, Logger),
                                       PriorityProcesses = p.Event.PriorityProcesses
                                   })
               .Do(_ => Process.GetProcesses().Foreach(p => Self.Tell(p)),
                    ex =>
                    {
                        MessageBox.Show($"Fehler beim Starten der Process Überwachung:{Environment.NewLine}{ex}", "Schwerer Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.FailFast(ex.Message, ex);
                    });
        }

        private IDisposable Handler(IObservable<StatePair<ProcessExitMessage, ProcessTrackerState>> obs)
        {
            return obs.Select(p => p.Event)
               .Select(obj => new ProcessStateChange(ProcessChange.Stopped, obj.Name, obj.Id, obj.Target))
               .ForwardToParent();
        }

        private static string FormatName(int id) => $"Process-{id}";

        public sealed record ProcessTrackerState(ProcessGartherer? Gartherer, ImmutableArray<string> Tracked, int ClientAffinity, int OperationSystemAffinity, ImmutableList<string> PriorityProcesses);
    }
}