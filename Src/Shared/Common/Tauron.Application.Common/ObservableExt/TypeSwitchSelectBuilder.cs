using System;
using System.Reactive.Linq;

namespace Tauron.ObservableExt;

[PublicAPI]
public readonly struct TypeSwitchSelectBuilder<TSource, TResult>
{
    private readonly ConditionalSelectBuilder<TSource, TResult> _registrations;

    public TypeSwitchSelectBuilder(ConditionalSelectBuilder<TSource, TResult> registrations)
        => _registrations = registrations;

    public TypeSwitchSelectBuilder<TSource, TResult> When<TType>(
        Func<IObservable<TType>, IObservable<TResult>> then)
        where TType : TSource
    {
        _registrations.Add(source => source is TType, observable => then(observable.Select(obj => (TType)obj!)));

        return this;
    }
}