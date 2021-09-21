using System;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.ObservableExt
{
    [PublicAPI]
    public static class TypeSwitchExtension
    {
        public static TypeSwitchTypeConfig<TSource> TypeSwitch<TSource>(this IObservable<TSource> observable)
            => new(observable.ConditionalSelect());
    }

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
}