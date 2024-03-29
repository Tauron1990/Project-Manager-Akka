﻿using System.Collections.Concurrent;
using System.Diagnostics;
using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public sealed class TimerScheduler : SchedulerBase, IDateTimeOffsetNowTimeProvider, IDisposable
{
    private static readonly Stopwatch Stopwatch = new();

    private readonly ConcurrentDictionary<string, Registration> _registrations = new(StringComparer.Ordinal);
    private readonly BlockingCollection<(Action, IDisposable)> _toRun = new();
    private int _isDiposed;

    public TimerScheduler(Config scheduler, ILoggingAdapter log)
        : base(scheduler, log)
    {
        Task.Factory.StartNew(
            () =>
            {
                foreach ((Action, IDisposable) action in _toRun.GetConsumingEnumerable())
                    try
                    {
                        action.Item1();
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception, "Error On Shedule Task");
                        action.Item2.Dispose();
                    }

                _toRun.Dispose();
            },
            TaskCreationOptions.LongRunning).Ignore();
    }

    protected override DateTimeOffset TimeNow => DateTimeOffset.Now;
    public override TimeSpan MonotonicClock => Stopwatch.Elapsed;
    public override TimeSpan HighResMonotonicClock => Stopwatch.Elapsed;

    public void Dispose()
    {
        Interlocked.Exchange(ref _isDiposed, 1);

        foreach (var registration in _registrations)
            registration.Value.Dispose();

        _registrations.Clear();
        _toRun.CompleteAdding();
    }

    protected override void InternalScheduleTellOnce(
        TimeSpan delay, ICanTell receiver, object message,
        IActorRef sender, ICancelable cancelable)
        => AddGeneric(() => receiver.Tell(message, sender), delay, Timeout.InfiniteTimeSpan, cancelable);

    protected override void InternalScheduleTellRepeatedly(
        TimeSpan initialDelay, TimeSpan interval,
        ICanTell receiver, object message, IActorRef sender, ICancelable cancelable)
        => AddGeneric(() => receiver.Tell(message, sender), initialDelay, interval, cancelable);

    protected override void InternalScheduleOnce(TimeSpan delay, Action action, ICancelable cancelable)
        => AddGeneric(action, delay, Timeout.InfiniteTimeSpan, cancelable);

    protected override void InternalScheduleOnce(TimeSpan delay, IRunnable action, ICancelable cancelable)
        => AddGeneric(action.Run, delay, Timeout.InfiniteTimeSpan, cancelable);

    protected override void InternalScheduleRepeatedly(
        TimeSpan initialDelay, TimeSpan interval, Action action,
        ICancelable cancelable)
        => AddGeneric(action, initialDelay, interval, cancelable);

    protected override void InternalScheduleRepeatedly(
        TimeSpan initialDelay, TimeSpan interval, IRunnable action,
        ICancelable cancelable)
        => AddGeneric(action.Run, initialDelay, interval, cancelable);

    private void AddGeneric(Action runner, TimeSpan delay, TimeSpan interval, ICancelable? cancelable)
    {
        if(_isDiposed == 1) return;

        var id = Guid.NewGuid().ToString();

        if(_registrations.ContainsKey(id))
            throw new InvalidOperationException("Guid Duplicate key in Scheduler");

        var dispoise = new Disposer();

        var registration = new Registration(
            () =>
            {
                try
                {
                    if(_toRun.IsAddingCompleted) return;

                    _toRun.Add((runner, dispoise));
                }
                catch (ObjectDisposedException) { }
            },
            delay,
            interval,
            cancelable,
            id,
            key => _registrations.TryRemove(key, out _));
        dispoise.Set(registration);

        _registrations[id] = registration;
    }


    internal class Disposer : IDisposable
    {
        private IDisposable? _disposable;

        public void Dispose()
            => _disposable?.Dispose();

        internal void Set(IDisposable disposable)
            => _disposable = disposable;
    }

    private class Registration : IDisposable
    {
        private readonly ICancelable? _cancelable;
        private readonly string _id;
        private readonly Action<string> _remove;
        private readonly Action _runner;
        private readonly Timer _timer;

        internal Registration(
            Action runner, TimeSpan delay, TimeSpan interval, ICancelable? cancelable, string id,
            Action<string> remove)
        {
            _runner = runner;
            _cancelable = cancelable;
            _id = id;
            _remove = remove;

            _timer = new Timer(Run, null, delay, interval);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void Run(object? state)
        {
            if(_cancelable?.IsCancellationRequested == true)
            {
                _timer.Dispose();
                _remove(_id);
            }

            _runner();
        }
    }
}