using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Features;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services
{
    //public abstract class ReportingActorOld : ObservableActor
    //{
    //    public static string GenralError = nameof(GenralError);

    //    [PublicAPI]
    //    protected void Receive<TMessage>(string name, Action<TMessage, Reporter> process) 
    //        where TMessage : IReporterMessage => Receive<TMessage>(obs => obs.Subscribe(m => TryExecute(m, name, process)));

    //    [PublicAPI]
    //    protected void ReceiveContinue<TMessage>(string name, Action<TMessage, Reporter> process)
    //        where TMessage : IDelegatingMessage => Receive<TMessage>(obs => obs.Subscribe(m => TryContinue(m, name, process)));

    //    protected virtual void TryExecute<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
    //        where TMessage : IReporterMessage
    //    {
    //        Log.Info("Enter Process {Name}", name);
    //        var reporter = Reporter.CreateReporter(Context);
    //        reporter.Listen(msg.Listner);

    //        try
    //        {
    //            process(msg, reporter);
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
    //            reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
    //        }
    //    }

    //    protected virtual void TryContinue<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process)
    //        where TMessage : IDelegatingMessage
    //    {
    //        Log.Info("Enter Process {Name}", name);
    //        try
    //        {
    //            process(msg, msg.Reporter);
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.Info);
    //            msg.Reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
    //        }
    //    }
    //}

    [PublicAPI]
    public abstract class ReportingActor<TState> : ActorFeatureBase<TState>
    {
        public static string GenralError = nameof(GenralError);

        //[PublicAPI]
        //protected void Receive<TMessage>(string name, Action<StatePair<TMessage, TState>, Reporter> process)
        //    where TMessage : IReporterMessage
        //    => Receive<TMessage>(obs => obs.SubscribeWithStatus(m => TryExecute(m, name, process)));

        //[PublicAPI]
        //protected void ReceiveContinue<TMessage>(string name, Action<StatePair<TMessage, TState>, Reporter> process)
        //    where TMessage : IDelegatingMessage
        //    => Receive<TMessage>(obs => obs.SubscribeWithStatus(m => TryContinue(m, name, process)));

        //protected void Receive<TMessage>(string name,
        //    Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
        //    where TMessage : IReporterMessage
        //    => Receive<TMessage>(obs => obs.SelectMany(m => TryExecute(m, name, process)));

        //[PublicAPI]
        //protected void ReceiveContinue<TMessage>(string name,
        //    Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
        //    where TMessage : IDelegatingMessage
        //    => Receive<TMessage>(obs => obs.SelectMany(m => TryContinue(m, name, process)));

        //protected Task<Unit> TryExecute<TMessage>(StatePair<TMessage, TState> msg, string name,
        //    Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
        //    where TMessage : IReporterMessage
        //{
        //    Log.Info("Enter Process {Name}", name);
        //    var reporter = Reporter.CreateReporter(Context);
        //    reporter.Listen(msg.Event.Listner);

        //    return process(reporter, Observable.Return(msg))
        //        .ToTask().ContinueWith(t =>
        //        {
        //            if (t.IsCompletedSuccessfully)
        //                return t.Result;

        //            if (t.IsFaulted && !reporter.IsCompled)
        //                reporter.Compled(
        //                    OperationResult.Failure(new Error(t.Exception?.Unwrap()?.Message, GenralError)));

        //            Log.Error((Exception?) t.Exception ?? new InvalidOperationException("Unkowen Error"),
        //                "Process Operation {Name} Failed {Info}", name, msg.Event.Info);
        //            return Unit.Default;
        //        }, TaskContinuationOptions.ExecuteSynchronously);
        //}

        //protected void TryExecute<TMessage>(StatePair<TMessage, TState> msg, string name,
        //    Action<StatePair<TMessage, TState>, Reporter> process)
        //    where TMessage : IReporterMessage
        //{
        //    Log.Info("Enter Process {Name}", name);
        //    var reporter = Reporter.CreateReporter(Context);
        //    reporter.Listen(msg.Event.Listner);

        //    try
        //    {
        //        process(msg, reporter);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e, "Process Operation {Name} Failed {Info}", name, msg.Event.Info);
        //        reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
        //    }
        //}

        //protected Task<Unit> TryContinue<TMessage>(StatePair<TMessage, TState> msg, string name,
        //    Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
        //    where TMessage : IDelegatingMessage
        //{
        //    Log.Info("Enter Process {Name}", name);
        //    var reporter = msg.Event.Reporter;

        //    return process(reporter, Observable.Return(msg))
        //        .ToTask().ContinueWith(t =>
        //        {
        //            if (t.IsCompletedSuccessfully)
        //                return t.Result;

        //            if (t.IsFaulted && !reporter.IsCompled)
        //                reporter.Compled(
        //                    OperationResult.Failure(new Error(t.Exception?.Unwrap()?.Message, GenralError)));

        //            Log.Error((Exception?) t.Exception ?? new InvalidOperationException("Unkowen Error"),
        //                "Continue Operation {Name} Failed {Info}", name, msg.Event.Info);
        //            return Unit.Default;
        //        }, TaskContinuationOptions.ExecuteSynchronously);
        //}

        //protected void TryContinue<TMessage>(StatePair<TMessage, TState> msg, string name,
        //    Action<StatePair<TMessage, TState>, Reporter> process)
        //    where TMessage : IDelegatingMessage
        //{
        //    Log.Info("Enter Process {Name}", name);
        //    try
        //    {
        //        process(msg, msg.Event.Reporter);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e, "Continue Operation {Name} Failed {Info}", name, msg.Event.Info);
        //        msg.Event.Reporter.Compled(
        //            OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
        //    }
        //}

        private void PrepareReceive<TMessage, TResult>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TResult?>> factory, Action<TResult?, Reporter> handler)
            where TMessage : IReporterMessage
        {
            Receive<TMessage>(
                obs => obs
                      .Do(m => Log.Info("Enter Operation {Name} -- {Info}", name, m.Event.Info))
                      .SelectMany(m =>
                                  {
                                      var reporter = Reporter.CreateReporter(Context);
                                      reporter.Listen(m.Event.Listner);
                                      var subject = Observable.Return(new ReporterEvent<TMessage, TState>(reporter, m));
                                      var signal = new Subject<Unit>();

                                      var disposable = new SingleAssignmentDisposable
                                                       {
                                                           Disposable = factory(subject).Subscribe(
                                                               r => handler(r, reporter),
                                                               e =>
                                                               {
                                                                   if (!reporter.IsCompled)
                                                                       reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message, GenralError)));

                                                                   Log.Error(e, "Process Operation {Name} Failed {Info}", name, m.Event.Info);
                                                               },
                                                               () => signal.OnCompleted())
                                                       };

                                      return signal
                                            .Finally(() => disposable.Dispose())
                                            .Finally(() => Log.Info("Exit Operation {Name} -- {Info}", name, m.Event.Info));
                                  }));
        }

        public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<IOperationResult?>> factory)
            where TMessage : IReporterMessage
            => PrepareReceive(name, factory, (result, reporter) =>
                                             {
                                                 if(!reporter.IsCompled && result != null)
                                                     reporter.Compled(result);
                                             });

        public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<Unit>> factory)
            where TMessage : IReporterMessage
            => PrepareReceive(name, factory, (_, _) => {});

        private IObservable<TResult> PrepareContinue<TMessage, TResult>(
            string name,
            IObservable<StatePair<TMessage, TState>> input, 
            Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TResult>> factory)
            where TMessage : IDelegatingMessage
        {
            return input
                  .Do(m => Log.Info("Enter Operation {Name} -- {Info}", name, m.Event.Info))
                  .SelectMany(m =>
                              {
                                  var reporter = m.Event.Reporter;
                                  var subject = Observable.Return(new ReporterEvent<TMessage, TState>(reporter, m));
                                  var signal = new Subject<TResult>();

                                  var disposable = new SingleAssignmentDisposable
                                                   {
                                                       Disposable = factory(subject).Subscribe(
                                                           r => signal.OnNext(r),
                                                           e =>
                                                           {
                                                               if (!reporter.IsCompled)
                                                                   reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message, GenralError)));

                                                               Log.Error(e, "Process Operation {Name} Failed {Info}", name, m.Event.Info);
                                                           },
                                                           () => signal.OnCompleted())
                                                   };

                                  return signal
                                        .Finally(() => disposable.Dispose())
                                        .Finally(() => Log.Info("Exit Operation {Name} -- {Info}", name, m.Event.Info));
                              });
        }

        public void TryContinue<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TState>> factory)
            where TMessage : IDelegatingMessage
            => Receive<TMessage>(obs => PrepareContinue(name, obs, factory));

        public void TryContinue<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<Unit>> factory)
            where TMessage : IDelegatingMessage
            => Receive<TMessage>(obs => PrepareContinue(name, obs, factory));


    }

    public sealed record ReporterEvent<TMessage, TState>(Reporter Reporter, TMessage Event, TState State, ITimerScheduler Timer)
    {
        public ReporterEvent(Reporter reporter, StatePair<TMessage, TState> @event)
            : this(reporter, @event.Event, @event.State, @event.Timers)
        {        }

        public ReporterEvent<TMessage, TState> CompledReporter(IOperationResult result)
        {
            Reporter.Compled(result);
            return this;
        }

        public ReporterEvent<TNewMessage, TState> New<TNewMessage>(TNewMessage newMessage)
            => new(Reporter, newMessage, State, Timer);

        public ReporterEvent<TNewMessage, TState> New<TNewMessage>(TNewMessage newMessage, TState state)
            => new(Reporter, newMessage, state, Timer);
    }
}