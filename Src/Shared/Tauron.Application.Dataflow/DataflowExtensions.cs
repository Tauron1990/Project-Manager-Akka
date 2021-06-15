using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;
using Tauron.Application.Dataflow.Internal;

namespace Tauron.Application.Dataflow
{
    [PublicAPI]
    public static class DataflowExtensions
    {
        public static ISourceBlock<TAccumulate> Aggregate<TSource, TAccumulate>(this ISourceBlock<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator) 
            => new AggregateSink<TSource, TAccumulate, TAccumulate>(seed, accumulator, accumulate => accumulate).LinkTo(source);

        public static ISourceBlock<TResult> Aggregate<TSource, TAccumulate, TResult>(this ISourceBlock<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector) 
            => new AggregateSink<TSource, TAccumulate, TResult>(seed, accumulator, resultSelector).LinkTo(source);

        public static ISourceBlock<TSource> Aggregate<TSource>(this ISourceBlock<TSource> source, Func<TSource, TSource, TSource> accumulator) 
            => new AggregateSink<TSource, TSource, TSource>(default, accumulator, dat => dat).LinkTo(source);

        public static ISourceBlock<bool> All<TSource>(this ISourceBlock<TSource> source, Func<TSource, bool> predicate)
            => new AllSink<TSource>(predicate).LinkTo(source);

        public static ISourceBlock<bool> Any<TSource>(this ISourceBlock<TSource> source)
            => new AnySink<TSource>(null).LinkTo(source);

        public static ISourceBlock<bool> Any<TSource>(this ISourceBlock<TSource> source, Func<TSource, bool> predicate)
            => new AnySink<TSource>(predicate).LinkTo(source);

        public static ISourceBlock<TResult> Start<TResult>(Func<TResult> function)
            => StartSource.Create(function);

        public static ISourceBlock<TResult> StartAsync<TResult>(Func<Task<TResult>> functionAsync)
            => StartSource.Create(functionAsync);

        public static ISourceBlock<TResult> Create<TResult>(Action<ITargetBlock<TResult>> creation)
        {
            var block = new StartUpBlock<TResult>();
            creation(block);
            return block;
        }

        public static ISourceBlock<TResult> Defer<TResult>(Func<ISourceBlock<TResult>> factory)
            => new DeferSource<TResult>(factory);


        public static IObservable<TResult> Defer<TResult>(Func<Task<IObservable<TResult>>> observableFactoryAsync)
            => new AsyncDeferSource<TResult>(observableFactoryAsync);
    }
}