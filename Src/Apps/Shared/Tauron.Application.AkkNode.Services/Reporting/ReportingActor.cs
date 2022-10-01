using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Features;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services.Reporting;

internal sealed partial class ReportingActorLog
{
    private readonly ILogger _logger;

    internal ReportingActorLog(ILogger logger)
        => _logger = logger;
    
    [LoggerMessage(EventId = 22, Level = LogLevel.Information, Message = "Enter Operation {name} -- {info}")]
    public partial void EnterOperation(string name, string info);

    [LoggerMessage(EventId = 23, Level = LogLevel.Error, Message = "Process Operation {name} Failed {info}")]
    public partial void FailedOperation(Exception ex, string name, string info);

    [LoggerMessage(EventId = 24, Level = LogLevel.Information, Message = "Exit Operation {name} -- {info}")]
    public partial void ExitOperation(string name, string info);
}

[PublicAPI]
public abstract class ReportingActor<TState> : ActorFeatureBase<TState>
{
    public const string GenralError = nameof(GenralError);

    private ReportingActorLog _logger = null!;

    protected override void Config()
    {
        _logger = new ReportingActorLog(Logger);
        base.Config();
    }
    
    private void PrepareReceive<TMessage, TResult>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TResult?>> factory, Action<TResult?, Reporter> handler)
        where TMessage : IReporterMessage
    {
        Receive<TMessage>(
            obs => obs
               .Do(m => _logger.EnterOperation(name, m.Event.Info))
               .SelectMany(
                    m =>
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

                                                     _logger.FailedOperation(e, name, m.Event.Info);
                                                 },
                                                 () => signal.OnCompleted())
                                         };

                        return signal
                           .Finally(() => disposable.Dispose())
                           .Finally(() => _logger.ExitOperation(name, m.Event.Info));
                    }));
    }

    public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<IOperationResult?>> factory)
        where TMessage : IReporterMessage
        => PrepareReceive(
            name,
            factory,
            (result, reporter) =>
            {
                if (!reporter.IsCompled && result != null)
                    reporter.Compled(result);
            });

    public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<Option<IOperationResult>>> factory)
        where TMessage : IReporterMessage
        => PrepareReceive(
            name,
            factory,
            (result, reporter) =>
            {
                if (!reporter.IsCompled && result.HasValue)
                    reporter.Compled(result.Value);
            });

    public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<ReporterEvent<IOperationResult, TState>>> factory)
        where TMessage : IReporterMessage
        => PrepareReceive(
            name,
            factory,
            (result, reporter) =>
            {
                if (!reporter.IsCompled && result?.Event != null)
                    reporter.Compled(result.Event);
            });

    public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<Unit>> factory)
        where TMessage : IReporterMessage
        => PrepareReceive(name, factory, (_, _) => { });

    private IObservable<TResult> PrepareContinue<TMessage, TResult>(
        string name,
        IObservable<StatePair<TMessage, TState>> input,
        Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TResult>> factory)
        where TMessage : IDelegatingMessage
    {
        return input
           .Do(m => _logger.EnterOperation(name, m.Event.Info))
           .SelectMany(
                m =>
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

                                                 _logger.FailedOperation(e, name, m.Event.Info);
                                             },
                                             () => signal.OnCompleted())
                                     };

                    return signal
                       .Finally(() => disposable.Dispose())
                       .Finally(() => _logger.ExitOperation(name, m.Event.Info));
                });
    }

    public void TryContinue<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TState>> factory)
        where TMessage : IDelegatingMessage
        => Receive<TMessage>(obs => PrepareContinue(name, obs, factory));

    public void TryContinue<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<Unit>> factory)
        where TMessage : IDelegatingMessage
        => Receive<TMessage>(obs => PrepareContinue(name, obs, factory));
}

public sealed record ReporterEvent<TMessage, TState>(
    Reporter Reporter, TMessage Event, TState State, ITimerScheduler Timer,
    IActorContext Context, IActorRef Sender, IActorRef Parent, IActorRef Self)
{
    public ReporterEvent(Reporter reporter, StatePair<TMessage, TState> @event)
        : this(reporter, @event.Event, @event.State, @event.Timers, @event.Context, @event.Sender, @event.Parent, @event.Self) { }

    public ReporterEvent<TMessage, TState> CompledReporter(IOperationResult result)
    {
        Reporter.Compled(result);

        return this;
    }

    public ReporterEvent<TNewMessage, TState> New<TNewMessage>(TNewMessage newMessage)
        => new(Reporter, newMessage, State, Timer, Context, Sender, Parent, Self);

    public ReporterEvent<TNewMessage, TState> New<TNewMessage>(TNewMessage newMessage, TState state)
        => new(Reporter, newMessage, state, Timer, Context, Sender, Parent, Self);
}