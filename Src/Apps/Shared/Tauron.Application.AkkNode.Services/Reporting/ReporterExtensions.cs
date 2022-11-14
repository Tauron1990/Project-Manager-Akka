using System;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.Application.AkkaNode.Services.Reporting;

[PublicAPI]
public static class ReporterExtensions
{
    public static IObservable<ReporterEvent<TMessage, TState>> Report<TMessage, TState>(this IObservable<ReporterEvent<TMessage, TState>> input, string message)
        where TMessage : IReporterMessage
        => input.Select(
            i =>
            {
                i.Reporter.Send(message);

                return i;
            });

    public static IObservable<ReporterEvent<TMessage, TState>> Report<TMessage, TState>(this IObservable<ReporterEvent<TMessage, TState>> input, Func<string> message)
        where TMessage : IReporterMessage
        => input.Select(
            i =>
            {
                i.Reporter.Send(message());

                return i;
            });

    public static IObservable<ReporterEvent<TMessage, TState>> Report<TMessage, TState>(this IObservable<ReporterEvent<TMessage, TState>> input, Func<TMessage, string> message)
        where TMessage : IReporterMessage
        => input.Select(
            i =>
            {
                i.Reporter.Send(message(i.Event));

                return i;
            });
}