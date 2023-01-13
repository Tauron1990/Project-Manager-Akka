using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Akka.Actor;
using Akka.Actor.Internal;
using JetBrains.Annotations;
using IScheduler = System.Reactive.Concurrency.IScheduler;

namespace Tauron.TAkka;

[PublicAPI]
public sealed class ActorScheduler : LocalScheduler
{
    private readonly IActorRef _targetActor;

    private ActorScheduler(IActorRef target) => _targetActor = target;

    private ActorScheduler()
        : this(ObservableActor.ExposedContext.Self) { }

    public static IScheduler CurrentSelf => new ActorScheduler();

    public static IScheduler From(IActorRef actor) => new ActorScheduler(actor);

    public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        TimeSpan target = Scheduler.Normalize(dueTime);

        SingleAssignmentDisposable disposable = new();

        void TryRun()
        {
            if(disposable.IsDisposed) return;

            disposable.Disposable = action(this, state);
        }

        if(target == TimeSpan.Zero)
        {
            ActorCell? currentCell = InternalCurrentActorCellKeeper.Current;
            if(currentCell != null && currentCell.Self.Equals(_targetActor))
                TryRun();
            else
                _targetActor.Tell(new ObservableActor.TransmitAction(TryRun));
        }
        else
        {
            var timerDispose = new SingleAssignmentDisposable();
            Timer timer = new(
                timerState =>
                {
                    _targetActor.Tell(new ObservableActor.TransmitAction(TryRun));
                    ((IDisposable)timerState!).Dispose();
                },
                timerDispose,
                dueTime,
                Timeout.InfiniteTimeSpan);

            timerDispose.Disposable = timer;
        }

        return disposable;
    }
}