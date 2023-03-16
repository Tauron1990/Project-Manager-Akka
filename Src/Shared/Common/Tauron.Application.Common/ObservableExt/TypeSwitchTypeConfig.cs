using System;

namespace Tauron.ObservableExt;

[PublicAPI]
public readonly struct TypeSwitchTypeConfig<TSource>
{
    private readonly ConditionalSelectTypeConfig<TSource> _observable;

    public TypeSwitchTypeConfig(ConditionalSelectTypeConfig<TSource> observable) => _observable = observable;

    public IObservable<TResult> ToResult<TResult>(Action<TypeSwitchSelectBuilder<TSource, TResult>> builder)
    {
        return _observable.ToResult<TResult>(
            selectBuilder =>
            {
                var setup = new TypeSwitchSelectBuilder<TSource, TResult>(selectBuilder);
                builder(setup);
            });
    }

    public IObservable<TSource> ToSame(Action<TypeSwitchSelectBuilder<TSource, TSource>> builder)
        => ToResult(builder);
}