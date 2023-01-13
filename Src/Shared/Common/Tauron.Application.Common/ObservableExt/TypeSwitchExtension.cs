using System;
using JetBrains.Annotations;

namespace Tauron.ObservableExt;

[PublicAPI]
public static class TypeSwitchExtension
{
    public static TypeSwitchTypeConfig<TSource> TypeSwitch<TSource>(this IObservable<TSource> observable)
        => new(observable.ConditionalSelect());
}