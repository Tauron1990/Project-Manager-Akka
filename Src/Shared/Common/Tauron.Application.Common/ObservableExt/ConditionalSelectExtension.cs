using System;
using System.Diagnostics;

namespace Tauron.ObservableExt;

[PublicAPI]
[DebuggerStepThrough]
public static class ConditionalSelectExtension
{
    public static ConditionalSelectTypeConfig<TSource> ConditionalSelect<TSource>(this IObservable<TSource> observable)
        => new(observable);
}