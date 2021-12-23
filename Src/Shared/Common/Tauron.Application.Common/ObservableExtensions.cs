using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Stl;

namespace Tauron;

[PublicAPI]
[DebuggerStepThrough]
public static class ObservableExtensions
{
    #region Common

    /// <summary>
    ///     Group observable sequence into buffers separated by periods of calm
    /// </summary>
    /// <param name="source">Observable to buffer</param>
    /// <param name="calmDuration">Duration of calm after which to close buffer</param>
    /// <param name="maxCount">Max size to buffer before returning</param>
    /// <param name="maxDuration">Max duration to buffer before returning</param>
    public static IObservable<IList<T>> BufferUntilCalm<T>(this IObservable<T> source, TimeSpan calmDuration, int? maxCount = null, TimeSpan? maxDuration = null)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var closes = source.Throttle(calmDuration);
        if (maxCount != null)
        {
            var overflows = source.Where((_, index) => index + 1 >= maxCount);
            closes = closes.Amb(overflows);
        }

        if (maxDuration != null)
        {
            var ages = source.Delay(maxDuration.Value);
            closes = closes.Amb(ages);
        }

        return source.Window(() => closes).SelectMany(window => window.ToList());
    }

    public static IObservable<TOutput> CatchSafe<TInput, TOutput>(
        this IObservable<TInput> input,
        Func<TInput, IObservable<TOutput>> process,
        Func<TInput, Exception, IObservable<TOutput>> catcher)
    {
        return input.SelectMany(
            i =>
            {
                try
                {
                    return process(i)
                       .Catch<TOutput, Exception>(e => catcher(i, e));
                }
                catch (Exception e)
                {
                    return catcher(i, e);
                }
            });
    }

    public static IObservable<Option<TData>> Lookup<TKey, TData>(this IDictionary<TKey, TData> dic, TKey key)
        => Observable.Return(dic.TryGetValue(key, out var data) ? data.AsOption() : default);

    public static IObservable<TData> NotDefault<TData>(this IObservable<TData?> source)
        => source.Where(d => !Equals(d, default(TData)))!;

    public static IObservable<TData> NotNull<TData>(this IObservable<TData?> source)
        => source.Where(d => d is not null)!;

    public static IObservable<string> NotEmpty(this IObservable<string?> source)
        => source.Where(s => !string.IsNullOrWhiteSpace(s))!;

    public static IObservable<CallResult<TResult>> SelectSafe<TEvent, TResult>(
        this IObservable<TEvent> observable,
        Func<TEvent, TResult> selector)
    {
        return observable.Select<TEvent, CallResult<TResult>>(
            evt =>
            {
                try
                {
                    return new SucessCallResult<TResult>(selector(evt));
                }
                catch (Exception e)
                {
                    return new ErrorCallResult<TResult>(e);
                }
            });
    }

    public static IObservable<Exception> OnError<TResult>(this IObservable<CallResult<TResult>> observable)
        => observable.Where(cr => cr is ErrorCallResult<TResult>).Cast<ErrorCallResult<TResult>>()
           .Select(er => er.Error);

    public static IObservable<TResult> OnResult<TResult>(this IObservable<CallResult<TResult>> observable)
        => observable.Where(cr => cr is SucessCallResult<TResult>).Cast<SucessCallResult<TResult>>()
           .Select(sr => sr.Result);

    public static IObservable<TData> ConvertResult<TData, TResult>(this IObservable<CallResult<TResult>> result, Func<TResult, TData> onSucess, Func<Exception, TData> error)
        => result.Select(cr => cr.ConvertResult(onSucess, error));

    public static TData ConvertResult<TData, TResult>(this CallResult<TResult> result, Func<TResult, TData> onSucess, Func<Exception, TData> error)
    {
        return result switch
        {
            SucessCallResult<TResult> sucess => onSucess(sucess.Result),
            ErrorCallResult<TResult> err => error(err.Error),
            _ => throw new InvalidOperationException("Incompatiple Call Result")
        };
    }

    public static IObservable<Unit> ToUnit<TSource>(this IObservable<TSource> input)
        => input.Select(_ => Unit.Default);

    public static IObservable<TType> Isonlate<TType>(this IObservable<TType> obs) => obs.Publish().RefCount();

    public static IObservable<Unit> ToUnit<TMessage>(this IObservable<TMessage> source, Action<TMessage> action)
    {
        return source.Select(
            m =>
            {
                action(m);

                return Unit.Default;
            });
    }

    public static IObservable<Unit> ToUnit<TMessage>(this IObservable<TMessage> source, Action action)
    {
        return source.Select(
            _ =>
            {
                action();

                return Unit.Default;
            });
    }

    public static IObservable<Unit> ToUnit<TMessage>(this IObservable<TMessage> source, Func<Task> action)
    {
        return source.SelectMany(
            async _ =>
            {
                await action();

                return Unit.Default;
            });
    }

    public static IObservable<Unit> ToUnit<TMessage>(this IObservable<TMessage> source, Func<TMessage, Task> action)
    {
        return source.SelectMany(
            async m =>
            {
                await action(m);

                return Unit.Default;
            });
    }

    public static IObservable<TData> ApplyWhen<TData>(this IObservable<TData> obs, Func<TData, bool> when, Action<TData> apply)
        => obs.Select(
            d =>
            {
                if (when(d))
                    apply(d);

                return d;
            });

    #endregion

    #region AutoSubscribe

    public interface ISubscriptionStrategy
    {
        IObservable<T> Prepare<T>(IObservable<T> observable);

        bool ReSubscribe(Action subscription, Exception? cause);
    }

    private sealed class DefaultResubscriptionStrategy : ISubscriptionStrategy
    {
        internal static readonly ISubscriptionStrategy Inst = new DefaultResubscriptionStrategy();

        public IObservable<T> Prepare<T>(IObservable<T> observable) => observable;

        public bool ReSubscribe(Action subscription, Exception? cause)
        {
            if (cause is ObjectDisposedException) return false;

            subscription();

            return true;
        }
    }

    private sealed class AutoSubscribeObserver<TData> : IDisposable, IObserver<TData>
    {
        private readonly IObservable<TData> _data;
        private readonly Func<Exception, bool> _errorHandler;
        private readonly ISubscriptionStrategy _strategy;
        private readonly IObserver<TData> _target;

        private IDisposable? _current;

        internal AutoSubscribeObserver(IObservable<TData> data, Func<Exception, bool> errorHandler, ISubscriptionStrategy strategy, IObserver<TData> target)
        {
            _data = strategy.Prepare(data);
            _errorHandler = errorHandler;
            _strategy = strategy;
            _target = target;

            strategy.ReSubscribe(AddSubscription, null);
        }

        public void Dispose()
        {
            _current?.Dispose();
            _current = null;
        }

        public void OnCompleted()
        {
            using (this)
            {
                _target.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            if (_errorHandler(error) && _strategy.ReSubscribe(AddSubscription, error)) return;

            using (this)
            {
                _target.OnError(error);
            }
        }

        public void OnNext(TData value)
        {
            try
            {
                _target.OnNext(value);
            }
            catch (Exception e)
            {
                if (!_errorHandler(e))
                {
                    _target.OnError(e);
                    Dispose();
                }
            }
        }

        private void AddSubscription()
        {
            try
            {
                _current = _data.Subscribe(this);
            }
            catch
            {
                Dispose();

                throw;
            }
        }
    }

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, IObserver<TData> target, Func<Exception, bool>? errorHandler, ISubscriptionStrategy? strategy = null)
    {
        errorHandler ??= _ => true;
        strategy ??= DefaultResubscriptionStrategy.Inst;

        return new AutoSubscribeObserver<TData>(obs, errorHandler, strategy, target);
    }

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Action<TData> onNext, Action<Exception> onError, Action onCompled, Func<Exception, bool>? errorHandler = null, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create(onNext, onError, onCompled), errorHandler, strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Func<Exception, bool>? errorHandler, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create<TData>(_ => { }), errorHandler, strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Action<TData> onNext, Action onCompled, Func<Exception, bool>? errorHandler = null, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create(onNext, _ => { }, onCompled), errorHandler, strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Action<TData> onNext, Func<Exception, bool>? errorHandler, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create(onNext), errorHandler, strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, IObserver<TData> target, Action<Exception>? errorHandler = null, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, target, CreateErrorHandler(errorHandler), strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Action<TData> onNext, Action<Exception> onError, Action onCompled, Action<Exception>? errorHandler = null, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create(onNext, onError, onCompled), CreateErrorHandler(errorHandler), strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Action<Exception>? errorHandler = null, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create<TData>(_ => { }), CreateErrorHandler(errorHandler), strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Action<TData> onNext, Action onCompled, Action<Exception>? errorHandler = null, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create(onNext, _ => { }, onCompled), CreateErrorHandler(errorHandler), strategy);

    public static IDisposable AutoSubscribe<TData>(this IObservable<TData> obs, Action<TData> onNext, Action<Exception>? errorHandler = null, ISubscriptionStrategy? strategy = null)
        => AutoSubscribe(obs, Observer.Create(onNext), CreateErrorHandler(errorHandler), strategy);

    private static Func<Exception, bool>? CreateErrorHandler(Action<Exception>? handler)
    {
        if (handler == null) return null;

        return e =>
               {
                   handler(e);

                   return true;
               };
    }

    #endregion

    #region Pause

    public static IObservable<TData> TakeWhile<TData>(this IObservable<TData> input, IObservable<bool> condition)
        => input.CombineLatest(condition.StartWith(true), (d, b) => (Data: d, CanRun: b))
           .Where(p => p.CanRun)
           .Select(p => p.Data);

    #endregion
}