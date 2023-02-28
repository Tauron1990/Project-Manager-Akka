using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Stl;
using Tauron.Features;
using Tauron.Operations;

namespace Tauron.Application.AkkaNode.Services.Reporting;

[PublicAPI]
public abstract class ReportingActor<TState> : ActorFeatureBase<TState>
{
    public const string GenralError = nameof(GenralError);

    private ReportingActorLog _logger = null!;

    protected override void Config()
    {
        _logger = new ReportingActorLog(base.Logger);
        base.Config();
    }

    private void PrepareReceive<TMessage, TResult>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TResult?>> factory, Action<TResult?, Reporter> handler)
        where TMessage : IReporterMessage
    {
        Observ<TMessage>(
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
                                                     if(!reporter.IsCompled)
                                                         reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message, GenralError)));

                                                     _logger.FailedOperation(e, name, m.Event.Info);
                                                 },
                                                 () => signal.OnCompleted()),
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
                if(!reporter.IsCompled && result != null)
                    reporter.Compled(result);
            });

    public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<Option<IOperationResult>>> factory)
        where TMessage : IReporterMessage
        => PrepareReceive(
            name,
            factory,
            (result, reporter) =>
            {
                if(!reporter.IsCompled && result.HasValue)
                    reporter.Compled(result.Value);
            });

    public void TryReceive<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<ReporterEvent<IOperationResult, TState>>> factory)
        where TMessage : IReporterMessage
        => PrepareReceive(
            name,
            factory,
            (result, reporter) =>
            {
                if(!reporter.IsCompled && result?.Event != null)
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
                    Reporter reporter = m.Event.Reporter;
                    var subject = Observable.Return(new ReporterEvent<TMessage, TState>(reporter, m));
                    var signal = new Subject<TResult>();

                    var disposable = new SingleAssignmentDisposable
                                     {
                                         Disposable = factory(subject).Subscribe(
                                             r => signal.OnNext(r),
                                             e =>
                                             {
                                                 if(!reporter.IsCompled)
                                                     reporter.Compled(OperationResult.Failure(new Error(e.Unwrap()?.Message, GenralError)));

                                                 _logger.FailedOperation(e, name, m.Event.Info);
                                             },
                                             () => signal.OnCompleted()),
                                     };

                    return signal
                       .Finally(() => disposable.Dispose())
                       .Finally(() => _logger.ExitOperation(name, m.Event.Info));
                });
    }

    public void TryContinue<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<TState>> factory)
        where TMessage : IDelegatingMessage
        => Observ<TMessage>(obs => PrepareContinue(name, obs, factory));

    public void TryContinue<TMessage>(string name, Func<IObservable<ReporterEvent<TMessage, TState>>, IObservable<Unit>> factory)
        where TMessage : IDelegatingMessage
        => Observ<TMessage>(obs => PrepareContinue(name, obs, factory));
}