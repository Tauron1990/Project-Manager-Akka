using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Features;
using Tauron.TAkka;

namespace Tauron.Application.Workshop;

[PublicAPI]
public interface IWorkDistributor<in TInput>
{
    void PushWork(TInput workLoad);
}

public sealed class WorkDistributorFeature<TInput, TFinishMessage> : ActorFeatureBase<WorkDistributorFeature<TInput, TFinishMessage>.WorkDistributorFeatureState>
{
    [PublicAPI]
    public static IWorkDistributor<TInput> Create(Props worker, string workerName, TimeSpan timeout, string? name = null)
        => Create(ObservableActor.ExposedContext, worker, workerName, timeout, name);

    [PublicAPI]
    public static IWorkDistributor<TInput> Create(IActorRefFactory factory, Props worker, string workerName, TimeSpan timeout, string? name = null)
    {
        IActorRef actor = factory.ActorOf(
            name,
            Feature.Create(
                () => new WorkDistributorFeature<TInput, TFinishMessage>(),
                _ => new WorkDistributorFeatureState(new DistributorConfig(worker, workerName, timeout, 5))));

        return new WorkSender(actor);
    }

    protected override void ConfigImpl()
    {
        SupervisorStrategy = SupervisorStrategy.StoppingStrategy;

        Receive<Terminated>(
            obs => obs.Do(_ => Self.Tell(new CheckWorker()))
               .Select(
                    m => m.State with
                         {
                             Ready = m.State.Ready
                                .Where(a => !a.Equals(m.Event.ActorRef))
                                .Aggregate(ImmutableQueue<IActorRef>.Empty, (current, actorRef) => current.Enqueue(actorRef)),
                             Worker = m.State.Worker.Remove(m.Event.ActorRef),
                             Running = m.State.Running.Remove(m.Event.ActorRef)
                         }));

        Receive<CheckWorker>(
            obs => obs.Where(s => s.State.Worker.Count < s.State.Configuration.WorkerCount)
               .SelectMany(s => Observable.Repeat(new SetupWorker(), s.State.Configuration.WorkerCount - s.State.Worker.Count))
               .SubscribeWithStatus(m => Self.Tell(m)));

        Receive<SetupWorker>(
            obs => obs.Select(
                    s =>
                    {
                        (_, WorkDistributorFeatureState state, _) = s;
                        IActorRef? worker = Context.ActorOf(
                            state.Configuration.Worker,
                            $"{state.Configuration.WorkerName}-{state.WorkerId}");
                        Context.Watch(worker);

                        return (Worker: worker, State: state);
                    })
               .Select(
                    s => s.State with
                         {
                             Worker = s.State.Worker.Add(s.Worker),
                             Ready = s.State.Ready.Enqueue(s.Worker),
                             WorkerId = s.State.WorkerId + 1
                         }));

        Receive<WorkerTimeout>(
            obs => obs.Where(m => m.State.Running.Contains(m.Event.Worker))
               .SubscribeWithStatus(m => Context.Stop(m.Event.Worker)));

        Receive<TFinishMessage>(
            obs => obs.Where(m => m.State.Running.Contains(Sender))
               .Select(
                    m =>
                    {
                        (_, WorkDistributorFeatureState state, ITimerScheduler timerScheduler) = m;

                        if(state.PendingWorkload.IsEmpty)
                            return state with
                                   {
                                       Running = state.Running.Remove(Context.Sender),
                                       Ready = state.Ready.Enqueue(Context.Sender)
                                   };

                        var newQueue = state.PendingWorkload.Dequeue(out (TInput Workload, IActorRef Sender) work);

                        RunWork(work.Workload, Context.Sender, work.Sender, timerScheduler, state.Configuration.Timeout);

                        return state with { PendingWorkload = newQueue };
                    }));

        Receive<TInput>(
            obs => obs.Select(
                m =>
                {
                    (TInput input, WorkDistributorFeatureState state, ITimerScheduler timerScheduler) = m;

                    if(state.Ready.IsEmpty)
                        return state with { PendingWorkload = state.PendingWorkload.Enqueue((input, Context.Sender)) };

                    var newQueue = state.Ready.Dequeue(out IActorRef worker);
                    RunWork(input, worker, Context.Sender, timerScheduler, state.Configuration.Timeout);

                    return state with
                           {
                               Running = state.Running.Add(worker),
                               Ready = newQueue
                           };
                }));

        Self.Tell(new CheckWorker());
    }

    private static void RunWork(TInput input, IActorRef worker, IActorRef sender, ITimerScheduler timers, TimeSpan timeout)
    {
        worker.Tell(input, sender);
        timers.StartSingleTimer(worker, new WorkerTimeout(worker), timeout);
    }

    public sealed record DistributorConfig(Props Worker, string WorkerName, TimeSpan Timeout, int WorkerCount);

    public sealed record WorkDistributorFeatureState(
        DistributorConfig Configuration,
        ImmutableQueue<(TInput Workload, IActorRef Sender)> PendingWorkload, ImmutableList<IActorRef> Worker,
        ImmutableQueue<IActorRef> Ready, ImmutableList<IActorRef> Running, int WorkerId)
    {
        public WorkDistributorFeatureState(DistributorConfig config)
            : this(
                config,
                ImmutableQueue<(TInput, IActorRef)>.Empty,
                ImmutableList<IActorRef>.Empty,
                ImmutableQueue<IActorRef>.Empty,
                ImmutableList<IActorRef>.Empty,
                1) { }
    }

    private sealed class WorkSender : IWorkDistributor<TInput>
    {
        private readonly IActorRef _actor;

        internal WorkSender(IActorRef actor) => _actor = actor;

        public void PushWork(TInput workLoad) => _actor.Forward(workLoad);
    }

    private sealed record SetupWorker;

    private sealed record CheckWorker;

    private sealed record WorkerTimeout(IActorRef Worker);
}