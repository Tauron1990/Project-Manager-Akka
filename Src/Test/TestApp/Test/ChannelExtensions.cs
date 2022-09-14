using System;
using System.Threading.Channels;
using JetBrains.Annotations;
using TestApp.Test.Operator;

namespace TestApp.Test;

[PublicAPI]
public static class ChannelExtensions
{
    public static ChannelReader<TResult> SelectMany<TSource,TCollection,TResult> (
        this ChannelReader<TSource> source,
        Func<TSource,System.Collections.Generic.IEnumerable<TCollection>> collectionSelector,
        Func<TSource,TCollection,TResult> resultSelector);
    
    public static ChannelReader<TResult> SelectMany<TSource,TCollection,TResult> (
        this Channel<TSource> source,
        Func<TSource,System.Collections.Generic.IEnumerable<TCollection>> collectionSelector,
        Func<TSource,TCollection,TResult> resultSelector);
    
    public static ChannelReader<TIn> Where<TIn>(this Channel<TIn> channel, Func<TIn, bool> condition)
        => Where(channel.Reader, condition);

    public static ChannelReader<TIn> Where<TIn>(this ChannelReader<TIn> channel, Func<TIn, bool> condition)
        => new ChannelWhere<TIn>(channel, condition);
    
    public static ChannelReader<TOut> Select<TIn, TOut>(this Channel<TIn> reader, Func<TIn, TOut> transform)
        => reader.Reader.Select(transform);

    public static ChannelReader<TOut> Select<TIn, TOut>(this ChannelReader<TIn> reader, Func<TIn, TOut> transform)
        => new ChannelSelect<TIn, TOut>(reader, transform);
}