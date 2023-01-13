using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace Tauron.Application.CommonUI.AppCore;

public sealed class DispatcherScheduler : LocalScheduler
{
    private readonly IUIDispatcher _dispatcher;

    private DispatcherScheduler(IUIDispatcher dispatcher) => _dispatcher = dispatcher;

    public static IScheduler CurrentDispatcher { get; internal set; } = null!;

    public static IScheduler From(IUIDispatcher dispatcher) => new DispatcherScheduler(dispatcher);

    public override IDisposable Schedule<TState>(
        TState state, TimeSpan dueTime,
        Func<IScheduler, TState, IDisposable> action)
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
            _dispatcher.Post(TryRun);
        }
        else
        {
            var timerDispose = new SingleAssignmentDisposable();
            Timer timer = new(
                o =>
                {
                    _dispatcher.Post(TryRun);
                    ((IDisposable)o!).Dispose();
                },
                timerDispose,
                dueTime,
                Timeout.InfiniteTimeSpan);

            timerDispose.Disposable = timer;
        }

        return disposable;
    }
}