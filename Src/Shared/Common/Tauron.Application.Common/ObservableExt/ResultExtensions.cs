using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Errors;

namespace Tauron.ObservableExt;

[PublicAPI]
public static class ResultExtensions
{
    public static Result<TSource> Where<TSource>(this Result<TSource> source, Func<TSource, bool> predicate) =>
        source.Bind(d => predicate(d) ? Result.Ok(d) : new ConditionNotMet());

    public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, Result<TResult>> selector)
        => source.Bind(selector);

    public static Result<TResult> SelectMany<TSource, TCollection, TResult>(
        this Result<TSource> source, Func<TSource, Result<TCollection>> optionSelector, Func<TSource, TCollection, Result<TResult>> resultSelector) =>
        source.Bind(sourceValue => optionSelector(sourceValue).Select(collection => resultSelector(sourceValue, collection)));

    public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, TResult> selector)
        => source.Bind(selector);

    public static Result<TResult> SelectMany<TSource, TCollection, TResult>(
        this Result<TSource> source, Func<TSource, Result<TCollection>> optionSelector, Func<TSource, TCollection, TResult> resultSelector) =>
        source.Bind(sourceValue => optionSelector(sourceValue).Select(collection => resultSelector(sourceValue, collection)));
    
    public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, Func<Result<TResult>>> selector) 
        => source.Bind<TResult>(d => selector(d)());

    public static Result<TResult> SelectMany<TSource, TCollection, TResult>(
        this Result<TSource> source, Func<TSource, Result<TCollection>> optionSelector, Func<TSource, TCollection, Func<Result<TResult>>> resultSelector) =>
        source.Bind(sourceValue => optionSelector(sourceValue).Select(collection => resultSelector(sourceValue, collection)));
    
    public static Result Select<TSource>(this Result<TSource> source, Func<TSource, Action> selector)
        => source.Bind(d => Result.Try(selector(d)));

    public static Result SelectMany<TSource, TCollection>(
        this Result<TSource> source, Func<TSource, Result<TCollection>> optionSelector, Func<TSource, TCollection, Action> resultSelector) =>
        source.Bind(sourceValue => optionSelector(sourceValue).Select(collection => resultSelector(sourceValue, collection)));
    
    public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, Func<TResult>> selector)
        => source.Bind(d => Result.Try(selector(d)));

    public static Result<TResult> SelectMany<TSource, TCollection, TResult>(
        this Result<TSource> source, Func<TSource, Result<TCollection>> optionSelector, Func<TSource, TCollection, Func<TResult>> resultSelector) =>
        source.Bind(sourceValue => optionSelector(sourceValue).Select(collection => resultSelector(sourceValue, collection)));

    public static IObservable<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this IObservable<TSource> source, Func<TSource, Result<TCollection>> optionSelector,
        Func<TSource, TCollection, Result<TResult>> resultSelector)
    {
        return from sourceInst in source
            let opt = optionSelector(sourceInst)
            where opt.IsSuccess
            select resultSelector(sourceInst, opt.Value);
    }

    public static IObservable<TType> ToObservable<TType>(this Result<TType> option)
        => option.IsSuccess ? Observable.Return(option.Value) : Observable.Empty<TType>();

    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
        this IEnumerable<TSource> source, Func<TSource, Result<TCollection>> optionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        if(optionSelector is null)
            throw new ArgumentNullException(nameof(optionSelector));

        if(resultSelector is null)
            throw new ArgumentNullException(nameof(resultSelector));

        return from sourceInst in source
#pragma warning restore AV1250
            let opt = optionSelector(sourceInst)
            where opt.IsSuccess
            select resultSelector(sourceInst, opt.Value);
    }

    public static Result<Unit> ToUnit(this Result result)
        => result.Bind(() => Result.Ok(Unit.Default));
    
    public static void Run<TType>(this Result<TType> result, Action<TType> onSuccess, Action onFail)
    {
        if(result.IsSuccess)
            onSuccess(result.Value);
        else
            onFail();
    }

    public static void OnFail<TType>(this Result<TType> option, Action onFail)
    {
        if(!option.IsFailed)
            onFail();
    }

    public static Result<TNew> Bind<T, TNew>(this Result<T> input, Func<T, TNew> transfom)
        => input.Bind(d => Result.Try(() => transfom(d)));

    public static Caster<TType> Cast<TType>(this Result<TType> option)
        => new(option);

    public readonly struct Caster<TType>
    {
        private readonly Result<TType> _option;

        public Caster(Result<TType> option) => _option = option;

        public Result<TResult> To<TResult>()
            => _option.Bind(
                type =>
                {
                    if(type is TResult result)
                        return Result.Ok(result);

                    return new TypeMismatch();
                });
    }
}
