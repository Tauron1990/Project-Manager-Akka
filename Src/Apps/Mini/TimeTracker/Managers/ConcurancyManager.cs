using System;
using System.Reactive.Linq;
using System.Threading;

namespace TimeTracker.Managers
{
    public sealed class ConcurancyManager : IDisposable
    {
        private readonly SemaphoreSlim _asyncLock = new(Environment.ProcessorCount * 2);
        private readonly SemaphoreSlim _syncLock = new(1);

        //public IObservable<TData> AsyncCall<TData>(IObservable<TData> input) 
        //    => Observable.Create<TData>(o => input.Get(new SyncObserver<TData>(o, _asyncLock)));

        ////public Task WaitAsyncCallWindow()
        ////    => _asyncLock.WaitAsync();

        //private sealed class SyncObserver<TData> : ObserverBase<TData>
        //{
        //    private readonly IObserver<TData> _baseObserver;
        //    private readonly SemaphoreSlim _semaphore;

        //    public SyncObserver(IObserver<TData> baseObserver, SemaphoreSlim semaphore)
        //    {
        //        _baseObserver = baseObserver;
        //        _semaphore = semaphore;
        //    }

        //    protected override void OnNextCore(TData value)
        //    {
        //        try
        //        {
        //            _semaphore.WaitAsync()
        //                      .ContinueWith(t =>
        //                                    {
        //                                        if (t.IsCompletedSuccessfully)
        //                                        {
        //                                            try
        //                                            {
        //                                                _baseObserver.OnNext(value);
        //                                            }
        //                                            catch (Exception e)
        //                                            {
        //                                                _baseObserver.OnError(e);
        //                                            }
        //                                            finally
        //                                            {
        //                                                _semaphore.Release();
        //                                            }
        //                                        }
        //                                        else if(t.IsFaulted)
        //                                            _baseObserver.OnError(t.Exception.Unwrap()!);
        //                                        if(t.IsCanceled)
        //                                            _baseObserver.OnError(new TaskCanceledException(t));
        //                                    }, TaskContinuationOptions.RunContinuationsAsynchronously);
        //        }
        //        catch (Exception e)
        //        {
        //            _baseObserver.OnError(e);
        //        }
        //    }

        //    protected override void OnErrorCore(Exception error) => _baseObserver.OnError(error);

        //    protected override void OnCompletedCore() => _baseObserver.OnCompleted();
        //}

        public void Dispose()
        {
            _syncLock.Dispose();
            _asyncLock.Dispose();
        }

        public IObservable<TData> SyncCall<TData>(IObservable<TData> input, Func<IObservable<TData>, IObservable<TData>> runSync)
            => input.SelectMany(
                async d =>
                {
                    await _syncLock.WaitAsync();

                    try
                    {
                        return await runSync(Observable.Return(d));
                    }
                    finally
                    {
                        _syncLock.Release();
                    }
                });
    }

    //[PublicAPI]
    //public static class ConcurancyManagerExtensions
    //{
    //    public static IObservable<TData> SyncCall<TData>(this IObservable<TData> input, ConcurancyManager manager)
    //        => manager.SyncCall(input);

    //    public static IObservable<TData> AsyncCall<TData>(this IObservable<TData> input, ConcurancyManager manager)
    //        => manager.AsyncCall(input);
    //}
}