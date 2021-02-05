using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Actor.Internal;
using JetBrains.Annotations;
using IScheduler = System.Reactive.Concurrency.IScheduler;

namespace Tauron.Akka
{
    [PublicAPI]
    public sealed class ActorScheduler : LocalScheduler
    {
        private readonly IActorRef _targetActor;

        private ActorScheduler(IActorRef target) => _targetActor = target;

        private ActorScheduler()
            : this(ObservableActor.ExposedContext.Self)
        {
        }

        public static IScheduler CurrentSelf => new ActorScheduler();

        public static IScheduler From(IActorRef actor) => new ActorScheduler(actor);

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime,
            Func<IScheduler, TState, IDisposable> action)
        {
            var target = Scheduler.Normalize(dueTime);

            SingleAssignmentDisposable disposable = new();

            void TryRun()
            {
                if (disposable.IsDisposed) return;
                disposable.Disposable = action(this, state);
            }

            if (target == TimeSpan.Zero)
            {
                var currentCell = InternalCurrentActorCellKeeper.Current;
                if (currentCell != null && currentCell.Self.Equals(_targetActor))
                    TryRun();
                else
                    _targetActor.Tell(new ObservableActor.TransmitAction(TryRun));
            }
            else
            {
                var timerDispose = new SingleAssignmentDisposable();
                Timer timer = new(o =>
                {
                    _targetActor.Tell(new ObservableActor.TransmitAction(TryRun));
                    ((IDisposable) o!).Dispose();
                }, timerDispose, dueTime, Timeout.InfiniteTimeSpan);

                timerDispose.Disposable = timer;
            }

            return disposable;
        }
    }

    public static class ActorSchedulerExtensions
    {
        public static IObservable<TType> ObserveOnSelf<TType>(this IObservable<TType> observable)
            => observable.ObserveOn(ActorScheduler.CurrentSelf);
    }
}