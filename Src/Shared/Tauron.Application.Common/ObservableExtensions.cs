using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Internal;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Features;

namespace Tauron
{
    [PublicAPI, DebuggerStepThrough]
    public static class ObservableExtensions
    {
        public static IObservable<TOutput> CatchSafe<TInput, TOutput>(
            this IObservable<TInput> input, 
            Func<TInput, IObservable<TOutput>> process, 
            Func<TInput, Exception, IObservable<TOutput>> catcher)
        {
            return input.SelectMany(i =>
                                    {
                                        return process(i)
                                           .Catch<TOutput, Exception>(e => catcher(i, e));
                                    });
        }

        public static IObservable<TData?> Lookup<TKey, TData>(this IDictionary<TKey, TData> dic, TKey key) 
            => Observable.Return(dic.TryGetValue(key, out var data) ? data : default);

        public static IObservable<TData> NotDefault<TData>(this IObservable<TData?> source)
        {
            return source.Where(d => !Equals(d, default(TData)))!;
        }

        public static IObservable<TData> NotNull<TData>(this IObservable<TData?> source)
        {
            return source.Where(d => d != null)!;
        }

        public static IObservable<string> NotEmpty(this IObservable<string?> source)
        {
            return source.Where(s => !string.IsNullOrWhiteSpace(s))!;
        }

        public static IObservable<CallResult<TResult>> SelectSafe<TEvent, TResult>(this IObservable<TEvent> observable,
            Func<TEvent, TResult> selector)
        {
            return observable.Select<TEvent, CallResult<TResult>>(evt =>
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
        {
            return observable.Where(cr => cr is ErrorCallResult<TResult>).Cast<ErrorCallResult<TResult>>()
                .Select(er => er.Error);
        }

        public static IObservable<TResult> OnResult<TResult>(this IObservable<CallResult<TResult>> observable)
        {
            return observable.Where(cr => cr is SucessCallResult<TResult>).Cast<SucessCallResult<TResult>>()
                .Select(sr => sr.Result);
        }

        public static IObservable<TData> ConvertResult<TData, TResult>(this IObservable<CallResult<TResult>> result, Func<TResult, TData> onSucess, Func<Exception, TData> error)
        {
            return result.Select(cr => cr.ConvertResult(onSucess, error));
        }

        public static TData ConvertResult<TData, TResult>(this CallResult<TResult> result, Func<TResult, TData> onSucess, Func<Exception, TData> error)
        {
            return result switch
            {
                SucessCallResult<TResult> sucess => onSucess(sucess.Result),
                ErrorCallResult<TResult> err => error(err.Error),
                _ => throw new InvalidOperationException("Incompatiple Call Result")
            };
        }

        public static IObservable<IActorRef> NotNobody(this IObservable<IActorRef> observable)
            => observable.Where(a => !a.IsNobody());

        public static IObservable<Unit> ToUnit<TSource>(this IObservable<TSource> input)
            => input.Select(_ => Unit.Default);

        public static IObservable<TType> Isonlate<TType>(this IObservable<TType> obs) => obs.Publish().RefCount();

        public static IObservable<Unit> ToUnit<TMessage>(this IObservable<TMessage> source, Action<TMessage> action)
        {
            return source.Select(m =>
            {
                action(m);
                return Unit.Default;
            });
        }

        public static IObservable<Unit> ToUnit<TMessage>(this IObservable<TMessage> source, Action action)
        {
            return source.Select(_ =>
            {
                action();
                return Unit.Default;
            });
        }

        public static IObservable<Unit> ToUnit<TMessage>(this IObservable<TMessage> source, Func<Task> action)
        {
            return source.SelectMany(async _ =>
            {
                await action();
                return Unit.Default;
            });
        }

        public static IObservable<TData> ApplyWhen<TData>(this IObservable<TData> obs, Func<TData, bool> when, Action<TData> apply)
            => obs.Select(d =>
                          {
                              if (when(d))
                                  apply(d);
                              return d;
                          });


        public static IObservable<ITimerScheduler> StartPeriodicTimer(this IObservable<ITimerScheduler> timer, object key, object msg, TimeSpan interval)
            => timer.Select(t =>
                            {
                                t.StartPeriodicTimer(key, msg, interval);
                                return t;
                            });

        public static IObservable<StatePair<TEvent, TState>> StartPeriodicTimer<TEvent, TState>(this IObservable<StatePair<TEvent, TState>> obs, object key, object msg, TimeSpan interval)
            => obs.Select(p =>
                          {
                              p.Timers.StartPeriodicTimer(key, msg, interval);
                              return p;
                          });

        public static IObservable<TData> StartPeriodicTimer<TData>(this IObservable<TData> obs, Func<TData, ITimerScheduler> selector, object key, object msg, TimeSpan interval)
            => obs.Select(d =>
                          {
                              selector(d).StartPeriodicTimer(key, msg, interval);
                              return d;
                          });


        public static IObservable<ITimerScheduler> StartPeriodicTimer(this IObservable<ITimerScheduler> timer, object key, object msg, TimeSpan initialDelay, TimeSpan interval)
            => timer.Select(t =>
                            {
                                t.StartPeriodicTimer(key, msg, initialDelay, interval);
                                return t;
                            });

        public static IObservable<StatePair<TEvent, TState>> StartPeriodicTimer<TEvent, TState>(this IObservable<StatePair<TEvent, TState>> obs, object key, object msg, TimeSpan initialDelay,
            TimeSpan interval)
            => obs.Select(t =>
                          {
                              t.Timers.StartPeriodicTimer(key, msg, initialDelay, interval);
                              return t;
                          });

        public static IObservable<TData> StartPeriodicTimer<TData>(this IObservable<TData> obs, Func<TData, ITimerScheduler> selector, object key, object msg, TimeSpan initialDelay, TimeSpan interval)
            => obs.Select(t =>
                          {
                              selector(t).StartPeriodicTimer(key, msg, initialDelay, interval);
                              return t;
                          });

        public static IObservable<ITimerScheduler> StartSingleTimer(this IObservable<ITimerScheduler> timer, object key, object msg, TimeSpan timeout)
            => timer.Select(t =>
                            {
                                t.StartSingleTimer(key, msg, timeout);
                                return t;
                            });

        public static IObservable<StatePair<TEvent, TState>> StartSingleTimer<TEvent, TState>(this IObservable<StatePair<TEvent, TState>> obs, object key, object msg, TimeSpan timeout)
            => obs.Select(t =>
                          {
                              t.Timers.StartSingleTimer(key, msg, timeout);
                              return t;
                          });

        public static IObservable<TData> StartSingleTimer<TData>(this IObservable<TData> obs, Func<TData, ITimerScheduler> selector, object key, object msg, TimeSpan timeout)
            => obs.Select(t =>
                          {
                              selector(t).StartSingleTimer(key, msg, timeout);
                              return t;
                          });

        public static IObservable<ITimerScheduler> CancelTimer(this IObservable<ITimerScheduler> timer, object key)
            => timer.Select(t =>
                            {
                                t.Cancel(key);
                                return t;
                            });

        public static IObservable<StatePair<TEvent, TState>> CancelTimer<TEvent, TState>(this IObservable<StatePair<TEvent, TState>> obs, object key)
            => obs.Select(t =>
                          {
                              t.Timers.Cancel(key);
                              return t;
                          });

        public static IObservable<TData> CancelTimer<TData>(this IObservable<TData> obs, Func<TData, ITimerScheduler> selector, object key)
            => obs.Select(t =>
                          {
                              selector(t).Cancel(key);
                              return t;
                          });

        public static IObservable<ITimerScheduler> CancelAllTimers(this IObservable<ITimerScheduler> timer)
            => timer.Select(t =>
                            {
                                t.CancelAll();
                                return t;
                            });

        public static IObservable<StatePair<TEvent, TState>> CancelAllTimers<TEvent, TState>(this IObservable<StatePair<TEvent, TState>> obs)
            => obs.Select(t =>
                          {
                              t.Timers.CancelAll();
                              return t;
                          });

        public static IObservable<TData> CancelAllTimers<TData>(this IObservable<TData> obs, Func<TData, ITimerScheduler> selector)
            => obs.Select(t =>
                          {
                              selector(t).CancelAll();
                              return t;
                          });

        #region Send To Actor Dispose

        public static IDisposable ToSelf<TMessage>(this IObservable<TMessage> obs)
            => ToActor(obs, ObservableActor.ExposedContext.Self);

        public static IDisposable ToParent<TMessage>(this IObservable<TMessage> source)
            => ToParent(source, ObservableActor.ExposedContext);

        public static IDisposable ToParent<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.SubscribeWithStatus(m => context.Parent.Tell(m));

        public static IDisposable ToSender<TMessage>(this IObservable<TMessage> source)
            => ToSender(source, ObservableActor.ExposedContext);

        public static IDisposable ToSender<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.SubscribeWithStatus(m => context.Sender.Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, IActorRef target)
            => source.SubscribeWithStatus(m => target.Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, Func<IActorRef> target)
            => source.SubscribeWithStatus(m => target().Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, Func<IActorContext, IActorRef> target)
            => source.SubscribeWithStatus(m => target(ObservableActor.ExposedContext).Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target)
            => source.SubscribeWithStatus(m => target(m).Tell(m));

        public static IDisposable ToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target, Func<TMessage, object> selector)
            => source.SubscribeWithStatus(m => target(m).Tell(selector(m)));



        public static IDisposable ForwardToParent<TMessage>(this IObservable<TMessage> source)
            => ForwardToParent(source, ObservableActor.ExposedContext);

        public static IDisposable ForwardToParent<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.SubscribeWithStatus(m => context.Parent.Forward(m));

        public static IDisposable ForwardToSender<TMessage>(this IObservable<TMessage> source)
            => ForwardToSender(source, ObservableActor.ExposedContext);

        public static IDisposable ForwardToSender<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.SubscribeWithStatus(m => context.Sender.Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, IActorRef target)
            => source.SubscribeWithStatus(m => target.Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, Func<IActorRef> target)
            => source.SubscribeWithStatus(m => target().Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source,            Func<IActorContext, IActorRef> target)
            => source.SubscribeWithStatus(m => target(ObservableActor.ExposedContext).Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target)
            => source.SubscribeWithStatus(m => target(m).Forward(m));

        public static IDisposable ForwardToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target, Func<TMessage, object> selector)
            => source.SubscribeWithStatus(m => target(m).Forward(selector(m)));

        #endregion

        #region Send To Actor Unit

        public static IObservable<Unit> UToSelf<TMessage>(this IObservable<TMessage> obs)
            => UToActor(obs, ObservableActor.ExposedContext.Self);

        public static IObservable<Unit> UToParent<TMessage>(this IObservable<TMessage> source)
            => UToParent(source, ObservableActor.ExposedContext);

        public static IObservable<Unit> UToParent<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.ToUnit(m => context.Parent.Tell(m));

        public static IObservable<Unit> UToSender<TMessage>(this IObservable<TMessage> source)
            => UToSender(source, ObservableActor.ExposedContext);

        public static IObservable<Unit> UToSender<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.ToUnit(m => context.Sender.Tell(m));

        public static IObservable<Unit> UToActor<TMessage>(this IObservable<TMessage> source, IActorRef target)
            => source.ToUnit(m => target.Tell(m));

        public static IObservable<Unit> UToActor<TMessage>(this IObservable<TMessage> source, Func<IActorRef> target)
            => source.ToUnit(m => target().Tell(m));

        public static IObservable<Unit> UToActor<TMessage>(this IObservable<TMessage> source, Func<IActorContext, IActorRef> target)
            => source.ToUnit(m => target(ObservableActor.ExposedContext).Tell(m));

        public static IObservable<Unit> UToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target)
            => source.ToUnit(m => target(m).Tell(m));

        public static IObservable<Unit> UToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target, Func<TMessage, object> selector)
            => source.ToUnit(m => target(m).Tell(selector(m)));



        public static IObservable<Unit> UForwardToParent<TMessage>(this IObservable<TMessage> source)
            => UForwardToParent(source, ObservableActor.ExposedContext);

        public static IObservable<Unit> UForwardToParent<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.ToUnit(m => context.Parent.Forward(m));

        public static IObservable<Unit> UForwardToSender<TMessage>(this IObservable<TMessage> source)
            => UForwardToSender(source, ObservableActor.ExposedContext);

        public static IObservable<Unit> UForwardToSender<TMessage>(this IObservable<TMessage> source, IActorContext context)
            => source.ToUnit(m => context.Sender.Forward(m));

        public static IObservable<Unit> UForwardToActor<TMessage>(this IObservable<TMessage> source, IActorRef target)
            => source.ToUnit(m => target.Forward(m));

        public static IObservable<Unit> UForwardToActor<TMessage>(this IObservable<TMessage> source, Func<IActorRef> target)
            => source.ToUnit(m => target().Forward(m));

        public static IObservable<Unit> UForwardToActor<TMessage>(this IObservable<TMessage> source, Func<IActorContext, IActorRef> target)
            => source.ToUnit(m => target(ObservableActor.ExposedContext).Forward(m));

        public static IObservable<Unit> UForwardToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target)
            => source.ToUnit(m => target(m).Forward(m));

        public static IObservable<Unit> UForwardToActor<TMessage>(this IObservable<TMessage> source, Func<TMessage, IActorRef> target, Func<TMessage, object> selector)
            => source.ToUnit(m => target(m).Forward(selector(m)));

        #endregion

        #region Subscriptions

        public static IDisposable SubscribeWithStatus<TMessage>(this IObservable<TMessage> source, object? sucessMessage, Action<TMessage> onNext)
        {
            var cell = InternalCurrentActorCellKeeper.Current;

            if (cell == null)
                return source.Subscribe(onNext);
            var self = cell.Self;
            return source.Subscribe(onNext, 
                exception => self.Tell(new Status.Failure(exception)),
                () => self.Tell(new Status.Success(sucessMessage)));
        }

        public static IDisposable SubscribeWithStatus<TMessage>(this IObservable<TMessage> source, Action<TMessage> onNext)
            => SubscribeWithStatus(source, null, onNext);

        public static IDisposable SubscribeWithStatus<TMessage>(this IObservable<TMessage> source)
            => SubscribeWithStatus(source, null, _ => {});

        #endregion
    }
}