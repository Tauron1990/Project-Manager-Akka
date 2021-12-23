using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.ObservableExt;

[PublicAPI]
[DebuggerStepThrough]
public static class ConditionalSelectExtension
{
    public static ConditionalSelectTypeConfig<TSource> ConditionalSelect<TSource>(this IObservable<TSource> observable)
        => new(observable);
}

[PublicAPI]
[DebuggerStepThrough]
public sealed class ConditionalSelectTypeConfig<TSource>
{
    private readonly IObservable<TSource> _observable;

    public ConditionalSelectTypeConfig(IObservable<TSource> observable) => _observable = observable;

    public IObservable<TResult> ToResult<TResult>(Action<ConditionalSelectBuilder<TSource, TResult>> builder)
    {
        var setup = new ConditionalSelectBuilder<TSource, TResult>();
        builder(setup);

        return setup.Build(_observable).Merge();
    }

    public IObservable<TSource> ToSame(Action<ConditionalSelectBuilder<TSource, TSource>> builder)
        => ToResult(builder);
}

[PublicAPI]
[DebuggerStepThrough]
public sealed class ConditionalSelectBuilder<TSource, TResult>
{
    private readonly List<(Func<TSource, bool>, Func<IObservable<TSource>, IObservable<TResult>>)> _registrations =
        new();

    public void Add(Func<TSource, bool> when, Func<IObservable<TSource>, IObservable<TResult>> then)
    {
        _registrations.Add((when, then));
    }

    public ConditionalSelectBuilder<TSource, TResult> When(
        Func<TSource, bool> when,
        Func<IObservable<TSource>, IObservable<TResult>> then)
    {
        _registrations.Add((when, then));

        return this;
    }

    public IEnumerable<IObservable<TResult>> Build(IObservable<TSource> rawRource)
    {
        var source = rawRource.Publish().RefCount(_registrations.Count);

        foreach (var (when, then) in _registrations)
            yield return then(source.Where(when));
    }
}