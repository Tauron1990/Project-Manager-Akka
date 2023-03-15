using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Stl;

namespace Tauron.ObservableExt;

[PublicAPI]
public static class OptionExtensions
{
    public static Option<TSource> Where<TSource>(this Option<TSource> source, Func<TSource, bool> predicate)
    {
        if(predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return source.FlatSelect(sourceValue => predicate(sourceValue) ? sourceValue.AsOption() : Option<TSource>.None);
    }

    public static Option<TResult> Select<TSource, TResult>(this Option<TSource> source, Func<TSource, TResult> selector)
    {
        if(selector is null)
            throw new ArgumentNullException(nameof(selector));

        return source.Select(selector);
    }

    public static Option<TResult> SelectMany<TSource, TCollection, TResult>(
        this Option<TSource> source, Func<TSource, Option<TCollection>> optionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        if(optionSelector is null)
            throw new ArgumentNullException(nameof(optionSelector));

        if(resultSelector is null)
            throw new ArgumentNullException(nameof(resultSelector));

        return source.FlatSelect(sourceValue => optionSelector(sourceValue).Select(collection => resultSelector(sourceValue, collection)));
    }

    public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(
        this IObservable<TSource> source, Func<TSource, Option<TCollection>> optionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        if(optionSelector == null)
            throw new ArgumentNullException(nameof(optionSelector));

        if(resultSelector == null)
            throw new ArgumentNullException(nameof(resultSelector));

        return from sourceInst in source
               let opt = optionSelector(sourceInst)
               where opt.HasValue
               select resultSelector(sourceInst, opt.Value);
    }

    public static IObservable<TType> ToObservable<TType>(this Option<TType> option)
        => option.HasValue ? Observable.Return(option.Value) : Observable.Empty<TType>();

    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
        this IEnumerable<TSource> source, Func<TSource, Option<TCollection>> optionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        if(optionSelector is null)
            throw new ArgumentNullException(nameof(optionSelector));

        if(resultSelector is null)
            throw new ArgumentNullException(nameof(resultSelector));

        return from sourceInst in source
               #pragma warning restore AV1250
               let opt = optionSelector(sourceInst)
               where opt.HasValue
               select resultSelector(sourceInst, opt.Value);
    }

    public static void Run<TType>(this in Option<TType> option, Action<TType> onSuccess, Action onEmpty)
    {
        if(option.HasValue)
            onSuccess(option.Value);
        else
            onEmpty();
    }

    public static void OnEmpty<TType>(this Option<TType> option, Action onEmpty)
    {
        if(!option.HasValue)
            onEmpty();
    }

    public static Caster<TType> Cast<TType>(this Option<TType> option)
        => new(option);

    public readonly struct Caster<TType>
    {
        private readonly Option<TType> _option;

        public Caster(Option<TType> option) => _option = option;

        public Option<TResult> To<TResult>()
            => _option.FlatSelect(
                type =>
                {
                    if(type is TResult result)
                        return result.AsOption();

                    return Option<TResult>.None;
                });
    }
}