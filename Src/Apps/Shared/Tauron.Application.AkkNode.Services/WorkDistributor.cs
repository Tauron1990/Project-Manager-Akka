using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Streams.Implementation;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Features;

namespace Tauron.Application.AkkNode.Services
{
    [PublicAPI]
    public interface IWorkDistributor<in TInput>
    {
        void PushWork(TInput workLoad);
    }

    public sealed class WorkDistributorFeature<TInput, TFinishMessage> : ActorFeatureBase<WorkDistributorFeature<TInput, TFinishMessage>.WorkDistributorFeatureState>
    {
        public sealed record DistributorConfig(Props Worker, string WorkerName, TimeSpan Timeout, int WorkerCount);

        public sealed record WorkDistributorFeatureState(DistributorConfig Configuration, ImmutableQueue<(TInput, IActorRef)> PendingWorkload, ImmutableList<IActorRef> Worker,
            ImmutableQueue<IActorRef> Ready, ImmutableList<IActorRef> Running, int WorkerId)
        {
            public WorkDistributorFeatureState(DistributorConfig config)
                : this(config, ImmutableQueue<(TInput, IActorRef)>.Empty, ImmutableList<IActorRef>.Empty, ImmutableQueue<IActorRef>.Empty, ImmutableList<IActorRef>.Empty, 1) { }
        }

        [PublicAPI]
        public static IWorkDistributor<TInput> Create(Props worker, string workerName, TimeSpan timeout, string? name = null)
            => Create(ObservableActor.ExposedContext, worker, workerName, timeout, name);

        [PublicAPI]
        public static IWorkDistributor<TInput> Create(IActorRefFactory factory, Props worker, string workerName, TimeSpan timeout, string? name = null)
        {
            var actor = factory.ActorOf(name, 
                                        Feature.Create(new WorkDistributorFeature<TInput, TFinishMessage>(), 
                                                       () => new WorkDistributorFeatureState(new DistributorConfig(worker, workerName, timeout, 5))));

            return new WorkSender(actor);
        }

        protected override void Config()
        {
            SupervisorStrategy = SupervisorStrategy.StoppingStrategy;

            Receive<Terminated>(obs => obs.Do(_ => Self.Tell(new CheckWorker()))
                                          .Select(m => m.State with
                                                       {
                                                           Ready = m.State.Ready
                                                                    .Where(a => !a.Equals(m.Event.ActorRef))
                                                                    .Aggregate(ImmutableQueue<IActorRef>.Empty, (current, actorRef) => current.Enqueue(actorRef)),
                                                           Worker = m.State.Worker.Remove(m.Event.ActorRef),
                                                           Running = m.State.Running.Remove(m.Event.ActorRef)
                                                       }));

            Receive<CheckWorker>(obs => obs.Where(s => s.State.Worker.Count < s.State.Configuration.WorkerCount)
                                           .SelectMany(s => Observable.Repeat(new SetupWorker(), s.State.Worker.Count - s.State.Configuration.WorkerCount))
                                           .SubscribeWithStatus(m => Self.Tell(m)));

            Receive<SetupWorker>(obs => obs.Select(s =>
                                                   {
                                                       var (_, state, _) = s;
                                                       var worker = Context.ActorOf(state.Configuration.Worker, $"{state.Configuration.WorkerName}-{state.WorkerId}");
                                                       Context.Watch(worker);

                                                       return new {Worker = worker, State = state};
                                                   })
                                           .Select(s => s.State with
                                                        {
                                                            Worker = s.State.Worker.Add(s.Worker), 
                                                            Ready = s.State.Ready.Enqueue(s.Worker)
                                                        }));

            Receive<WorkerTimeout>(obs => obs.Where(m => m.State.Running.Contains(m.Event.Worker))
                                             .SubscribeWithStatus(m => Context.Stop(m.Event.Worker)));

            Receive<TFinishMessage>(obs => obs.Where(m => m.State.Running.Contains(Sender))
                                              .Select(m =>
                                                      {

                                                      }));

            Self.Tell(new CheckWorker());
        }

        private void RunWork(TInput input, IActorRef worker, IActorRef sender, ITimerScheduler timers, TimeSpan timeout)
        {
            worker.Tell(input, sender);
            timers.StartSingleTimer(worker, new WorkerTimeout(worker), timeout);
        }

        private sealed class WorkSender : IWorkDistributor<TInput>
        {
            private readonly IActorRef _actor;

            public WorkSender(IActorRef actor) => _actor = actor;

            public void PushWork(TInput workLoad) => _actor.Forward(workLoad);
        }

        private sealed record SetupWorker;

        private sealed record CheckWorker;

        private sealed record WorkerTimeout(IActorRef Worker);
    }

    [PublicAPI]
    public sealed class WorkDistributorÔld<TInput, TFinishMessage>
    {
    
                Receive<TFinishMessage>(WorkFinish);
                Receive<TInput>(PushWork);
            }

            private void WorkFinish(TFinishMessage msg)
            {
                if (!_running.Contains(Context.Sender)) return;

                if (_pendingWorkload.TryDequeue(out var elemnt))
                {
                    var (input, sender) = elemnt;
                    RunWork(input, Sender, sender);
                }
                else
                {
                    _running.Remove(Context.Sender);
                    _ready.Enqueue(Context.Sender);
                }
            }

            private void PushWork(TInput input)
            {
                if (_ready.TryDequeue(out var worker))
                {
                    RunWork(input, worker, Sender);
                    _running.Add(worker);
                }
                else
                    _pendingWorkload.Enqueue((input, Sender));
            }

protected override SupervisorStrategy SupervisorStrategy() => global::Akka.Actor.SupervisorStrategy.StoppingStrategy;


        }
    }
}