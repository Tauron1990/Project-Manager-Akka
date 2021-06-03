using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron;

namespace TimeTracker.Managers
{
    public sealed class ConcurancyManager : IDisposable
    {
        private readonly EventLoopScheduler _eventLoop = new();
        private readonly SemaphoreSlim _syncLock = new(Environment.ProcessorCount * 2);

        public IObservable<TData> SyncCall<TData>(IObservable<TData> input)
            => input.ObserveOn(_eventLoop);

        public IObservable<TData> AsyncCall<TData>(IObservable<TData> input) 
            => Observable.Create<TData>(o => input.Subscribe(new SyncObserver<TData>(o, _syncLock)));

        public Task WaitAsyncCallWindow()
            => _syncLock.WaitAsync();

        private sealed class SyncObserver<TData> : ObserverBase<TData>
        {
            private IObserver<TData> _baseObserver;
            private readonly SemaphoreSlim _semaphore;

            public SyncObserver(IObserver<TData> baseObserver, SemaphoreSlim semaphore)
            {
                _baseObserver = baseObserver;
                _semaphore = semaphore;
            }

            protected override void OnNextCore(TData value)
            {
                try
                {
                    _semaphore.WaitAsync()
                              .ContinueWith(t =>
                                            {
                                                if (t.IsCompletedSuccessfully)
                                                {
                                                    try
                                                    {
                                                        _baseObserver.OnNext(value);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        _baseObserver.OnError(e);
                                                    }
                                                    finally
                                                    {
                                                        _semaphore.Release();
                                                    }
                                                }
                                                else if(t.IsFaulted)
                                                    _baseObserver.OnError(t.Exception.Unwrap()!);
                                                if(t.IsCanceled)
                                                    _baseObserver.OnError(new TaskCanceledException(t));
                                            }, TaskContinuationOptions.RunContinuationsAsynchronously);
                }
                catch (Exception e)
                {
                    _baseObserver.OnError(e);
                }
            }

            protected override void OnErrorCore(Exception error) => _baseObserver.OnError(error);

            protected override void OnCompletedCore() => _baseObserver.OnCompleted();
        }

        public void Dispose()
        {
            _eventLoop.Dispose();
            _syncLock.Dispose();
        }
    }

    [PublicAPI]
    public static class ConcurancyManagerExtensions
    {
        public static IObservable<TData> SyncCall<TData>(this IObservable<TData> input, ConcurancyManager manager)
            => manager.SyncCall(input);

        public static IObservable<TData> AsyncCall<TData>(this IObservable<TData> input, ConcurancyManager manager)
            => manager.AsyncCall(input);
    }
}