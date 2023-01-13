using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TestApp.Test.Operator;

namespace TestApp.Test;

[PublicAPI]
public static class ChannelExtensions
{
    public static ChannelReader<TTarget> Cast<TFrom, TTarget>(this Channel<TFrom> reader)
        => reader.Reader.Cast<TFrom, TTarget>();

    public static ChannelReader<TTarget> Cast<TFrom, TTarget>(this ChannelReader<TFrom> reader)
        => new ChannelCastOperator<TFrom, TTarget>(reader);

    public static ChannelReader<TResult> SelectMany<TSource,TCollection,TResult> (
        this ChannelReader<TSource> source,
        Func<TSource, IObservable<TCollection>> collectionSelector,
        Func<TSource,TCollection,TResult> resultSelector)
        => ChannelSelectComplex<TSource, TResult, TCollection>.Create(source, collectionSelector, resultSelector);

    public static ChannelReader<TResult> SelectMany<TSource, TCollection, TResult>(
        this Channel<TSource> source,
        Func<TSource, IObservable<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
        => SelectMany(source.Reader, collectionSelector, resultSelector);
    
    public static ChannelReader<TResult> SelectMany<TSource,TCollection,TResult> (
        this ChannelReader<TSource> source,
        Func<TSource, Task<TCollection>> collectionSelector,
        Func<TSource,TCollection,TResult> resultSelector)
        => ChannelSelectComplex<TSource, TResult, TCollection>.Create(source, collectionSelector, resultSelector);

    public static ChannelReader<TResult> SelectMany<TSource, TCollection, TResult>(
        this Channel<TSource> source,
        Func<TSource, Task<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
        => SelectMany(source.Reader, collectionSelector, resultSelector);
    
    public static ChannelReader<TResult> SelectMany<TSource,TCollection,TResult> (
        this ChannelReader<TSource> source,
        Func<TSource,IEnumerable<TCollection>> collectionSelector,
        Func<TSource,TCollection,TResult> resultSelector)
        => ChannelSelectComplex<TSource, TResult, TCollection>.Create(source, collectionSelector, resultSelector);

    public static ChannelReader<TResult> SelectMany<TSource, TCollection, TResult>(
        this Channel<TSource> source,
        Func<TSource, IEnumerable<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
        => SelectMany(source.Reader, collectionSelector, resultSelector);
    
    public static ChannelReader<TIn> Where<TIn>(this Channel<TIn> channel, Func<TIn, bool> condition)
        => Where(channel.Reader, condition);

    public static ChannelReader<TIn> Where<TIn>(this ChannelReader<TIn> channel, Func<TIn, bool> condition)
        => new ChannelWhere<TIn>(channel, condition);
    
    public static ChannelReader<TOut> Select<TIn, TOut>(this Channel<TIn> reader, Func<TIn, TOut> transform)
        => reader.Reader.Select(transform);

    public static ChannelReader<TOut> Select<TIn, TOut>(this ChannelReader<TIn> reader, Func<TIn, TOut> transform)
        => new ChannelSelect<TIn, TOut>(reader, transform);
}