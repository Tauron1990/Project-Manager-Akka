using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;
using Tauron.Application.Dataflow.Internal;

namespace Tauron.Application.Dataflow
{
    [PublicAPI]
    public static class Dataflow
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

        public static ISourceBlock<TResult> CreateAsync<TResult>(Func<ITargetBlock<TResult>, Task> creation)
        {
            return new TaskSource<TResult>(Task.Run<ISourceBlock<TResult>>(async () =>
                                           {
                                               var block = new StartUpBlock<TResult>();
                                               await creation(block);
                                               return block;
                                           }));
        }

        public static ISourceBlock<TResult> Defer<TResult>(Func<ISourceBlock<TResult>> factory)
            => new DeferSource<TResult>(factory);

        public static ISourceBlock<TResult> Defer<TResult>(Func<Task<ISourceBlock<TResult>>> observableFactoryAsync)
            => new AsyncDeferSource<TResult>(observableFactoryAsync);

        public static ISourceBlock<TResult> Empty<TResult>() 
            => StartSource.Empty<TResult>();

        public static ISourceBlock<TResult> Empty<TResult>(TResult witness) 
            => StartSource.Empty<TResult>();

        public static ISourceBlock<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector)
            => new GenerateSource<TState, TResult>(initialState, condition, iterate, resultSelector, null).Start();

        public static ISourceBlock<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, TimeSpan interval)
            => new GenerateSource<TState, TResult>(initialState, condition, iterate, resultSelector, interval);

        public static ISourceBlock<TResult> Never<TResult>() => new WriteOnceBlock<TResult>(null);

        public static ISourceBlock<TResult> Never<TResult>(TResult witness) => new WriteOnceBlock<TResult>(null);

        public static ISourceBlock<int> Range(int start, int count)
        {
            var max = start + (long)count - 1;
            if (count < 0 || max > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(count));
            return new GenerateSource<int, int>(start - 1, i => i < start + count, i => i + 1, i => i, null);
        }

        public static ISourceBlock<TResult> Repeat<TResult>(TResult value, int repeatCount)
            => new RepeatSource<TResult>(value, repeatCount);

        public static ISourceBlock<TResult> Return<TResult>(TResult value)
            => StartSource.Create(value);

        public static ISourceBlock<TResult> Throw<TResult>(Exception exception)
            => StartSource.Create<TResult>(exception);

        public static ISourceBlock<TResult> Throw<TResult>(Exception exception, TResult witness)
            => StartSource.Create<TResult>(exception);

        public static ISourceBlock<TResult> Using<TResult, TResource>(Func<TResource> resourceFactory, Func<TResource, ISourceBlock<TResult>> sourceFactory)
            where TResource : IDisposable
            => new UsingSource<TResource, TResult>(resourceFactory, sourceFactory);

        public static ISourceBlock<TResult> Using<TResult, TResource>(Func<Task<TResource>> resourceFactory, Func<TResource, Task<ISourceBlock<TResult>>> sourceFactory)
            where TResource : IDisposable
            => new UsingSource<TResource, TResult>(resourceFactory, sourceFactory);

        public static Task ForEachAsync<TSource>(this ISourceBlock<TSource> source, Action<TSource> onNext)
            => new ActionTask<TSource>(source, onNext, CancellationToken.None);

        public static Task ForEachAsync<TSource>(this ISourceBlock<TSource> source, Action<TSource> onNext, CancellationToken cancellationToken)
            => new ActionTask<TSource>(source, onNext, cancellationToken);

        public static Task ForEachAsync<TSource>(this ISourceBlock<TSource> source, Action<TSource, int> onNext)
            => new ActionTask<TSource>(source, onNext, CancellationToken.None);

        public static Task ForEachAsync<TSource>(this ISourceBlock<TSource> source, Action<TSource, int> onNext, CancellationToken cancellationToken)
            => new ActionTask<TSource>(source, onNext, cancellationToken);

        public static ISourceBlock<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, ISourceBlock<TResult>> sources, ISourceBlock<TResult> defaultSource)
            where TValue : notnull
            => new CaseSource<TValue, TResult>(selector, sources, defaultSource);

        public static ISourceBlock<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, ISourceBlock<TResult>> sources)
            where TValue : notnull
            => new CaseSource<TValue, TResult>(selector, sources, Empty<TResult>());
    }
}