using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Tauron.ObservableExt;

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