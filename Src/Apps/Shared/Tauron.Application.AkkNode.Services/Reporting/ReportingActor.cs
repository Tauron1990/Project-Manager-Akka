using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
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

        [PublicAPI]
        protected void Receive<TMessage>(string name, Action<StatePair<TMessage, TState>, Reporter> process)
            where TMessage : IReporterMessage
            => Receive<TMessage>(obs => obs.SubscribeWithStatus(m => TryExecute(m, name, process)));

        [PublicAPI]
        protected void ReceiveContinue<TMessage>(string name, Action<StatePair<TMessage, TState>, Reporter> process)
            where TMessage : IDelegatingMessage
            => Receive<TMessage>(obs => obs.SubscribeWithStatus(m => TryContinue(m, name, process)));

        protected void Receive<TMessage>(string name,
            Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
            where TMessage : IReporterMessage
            => Receive<TMessage>(obs => obs.SelectMany(m => TryExecute(m, name, process)));

        [PublicAPI]
        protected void ReceiveContinue<TMessage>(string name,
            Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
            where TMessage : IDelegatingMessage
            => Receive<TMessage>(obs => obs.SelectMany(m => TryContinue(m, name, process)));

        protected Task<Unit> TryExecute<TMessage>(StatePair<TMessage, TState> msg, string name,
            Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
            where TMessage : IReporterMessage
        {
            Log.Info("Enter Process {Name}", name);
            var reporter = Reporter.CreateReporter(Context);
            reporter.Listen(msg.Event.Listner);

            return process(reporter, Observable.Return(msg))
                .ToTask().ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                        return t.Result;

                    if (t.IsFaulted && !reporter.IsCompled)
                        reporter.Compled(
                            OperationResult.Failure(new Error(t.Exception?.Unwrap()?.Message, GenralError)));

                    Log.Error((Exception?) t.Exception ?? new InvalidOperationException("Unkowen Error"),
                        "Process Operation {Name} Failed {Info}", name, msg.Event.Info);
                    return Unit.Default;
                }, TaskContinuationOptions.ExecuteSynchronously);
        }

        protected void TryExecute<TMessage>(StatePair<TMessage, TState> msg, string name,
            Action<StatePair<TMessage, TState>, Reporter> process)
            where TMessage : IReporterMessage
        {
            Log.Info("Enter Process {Name}", name);
            var reporter = Reporter.CreateReporter(Context);
            reporter.Listen(msg.Event.Listner);

            try
            {
                process(msg, reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Process Operation {Name} Failed {Info}", name, msg.Event.Info);
                reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
            }
        }

        protected Task<Unit> TryContinue<TMessage>(StatePair<TMessage, TState> msg, string name,
            Func<Reporter, IObservable<StatePair<TMessage, TState>>, IObservable<Unit>> process)
            where TMessage : IDelegatingMessage
        {
            Log.Info("Enter Process {Name}", name);
            var reporter = msg.Event.Reporter;

            return process(reporter, Observable.Return(msg))
                .ToTask().ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                        return t.Result;

                    if (t.IsFaulted && !reporter.IsCompled)
                        reporter.Compled(
                            OperationResult.Failure(new Error(t.Exception?.Unwrap()?.Message, GenralError)));

                    Log.Error((Exception?) t.Exception ?? new InvalidOperationException("Unkowen Error"),
                        "Continue Operation {Name} Failed {Info}", name, msg.Event.Info);
                    return Unit.Default;
                }, TaskContinuationOptions.ExecuteSynchronously);
        }

        protected void TryContinue<TMessage>(StatePair<TMessage, TState> msg, string name,
            Action<StatePair<TMessage, TState>, Reporter> process)
            where TMessage : IDelegatingMessage
        {
            Log.Info("Enter Process {Name}", name);
            try
            {
                process(msg, msg.Event.Reporter);
            }
            catch (Exception e)
            {
                Log.Error(e, "Continue Operation {Name} Failed {Info}", name, msg.Event.Info);
                msg.Event.Reporter.Compled(
                    OperationResult.Failure(new Error(e.Unwrap()?.Message ?? "Unkowen", GenralError)));
            }
        }
    }
}