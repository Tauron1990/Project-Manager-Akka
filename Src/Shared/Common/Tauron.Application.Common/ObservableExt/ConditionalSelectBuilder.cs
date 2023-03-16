using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Tauron.ObservableExt;

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