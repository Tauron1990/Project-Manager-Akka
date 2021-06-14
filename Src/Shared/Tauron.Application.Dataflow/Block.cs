using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;

namespace Tauron.Application.Dataflow
{
    public static class ActionBlock
    {
        public static ActionBlock<TInput> Create<TInput>(Action<TInput> action)
            => new(action);

        public static ActionBlock<TInput> Create<TInput>(Action<TInput> action, ExecutionDataflowBlockOptions options)
            => new(action, options);

        public static ActionBlock<TInput> Create<TInput>(Func<TInput, Task> action)
            => new(action);

        public static ActionBlock<TInput> Create<TInput>(Func<TInput, Task> action, ExecutionDataflowBlockOptions options)
            => new(action, options);

        public static ActionBlock<TInput> Create<TInput>(Action<TInput> action, int maxDegreeOfParallelism)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism });

        public static ActionBlock<TInput> Create<TInput>(Func<TInput, Task> action, int maxDegreeOfParallelism)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism});

        public static ActionBlock<TInput> Create<TInput>(Action<TInput> action, int maxDegreeOfParallelism, int buond)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, BoundedCapacity =  buond});

        public static ActionBlock<TInput> Create<TInput>(Func<TInput, Task> action, int maxDegreeOfParallelism, int buond)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, BoundedCapacity = buond });
    }

    public static class TransformBlock
    {
        public static TransformBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, TOutput> action)
            => new(action);

        public static TransformBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, TOutput> action, ExecutionDataflowBlockOptions options)
            => new(action, options);

        public static TransformBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, Task<TOutput>> action)
            => new(action);

        public static TransformBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, Task<TOutput>> action, ExecutionDataflowBlockOptions options)
            => new(action, options);

        public static TransformBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, TOutput> action, int maxDegreeOfParallelism)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism });

        public static TransformBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, Task<TOutput>> action, int maxDegreeOfParallelism)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism });

    }

    public static class BufferBlock
    {
        public static BufferBlock<TInput> Create<TInput>()
            => new();

        public static BufferBlock<TInput> Create<TInput>(GroupingDataflowBlockOptions options)
            => new (options);

        public static BufferBlock<TInput> Create<TInput>(int buond)
            => new(new GroupingDataflowBlockOptions { BoundedCapacity = buond });
    }

    public static class BatchBlock
    {
        public static BatchBlock<TInput> Create<TInput>(int batchSize)
            => new(batchSize);

        public static BatchBlock<TInput> Create<TInput>(int batchSize, GroupingDataflowBlockOptions options)
            => new(batchSize, options);

        public static BatchBlock<TInput> Create<TInput>(int batchSize, int buond)
            => new(batchSize, new GroupingDataflowBlockOptions { BoundedCapacity = buond });
    }

    public static class BroadcastBlock
    {
        public static BroadcastBlock<TInput> Create<TInput>()
            => new(t => t);

        public static BroadcastBlock<TInput> Create<TInput>(Func<TInput, TInput> cloningFunction)
            => new(cloningFunction);

        public static BroadcastBlock<TInput> Create<TInput>(Func<TInput, TInput> cloningFunction, DataflowBlockOptions options)
            => new(cloningFunction, options);

        public static BroadcastBlock<TInput> Create<TInput>(Func<TInput, TInput> cloningFunction, int buond)
            => new(cloningFunction, new DataflowBlockOptions { BoundedCapacity = buond });
    }

    public static class TransformManyBlock
    {
        public static TransformManyBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, IEnumerable<TOutput>> action)
            => new(action);

        public static TransformManyBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, IEnumerable<TOutput>> action, ExecutionDataflowBlockOptions options)
            => new(action, options);

        public static TransformManyBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, Task<IEnumerable<TOutput>>> action)
            => new(action);

        public static TransformManyBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, Task<IEnumerable<TOutput>>> action, ExecutionDataflowBlockOptions options)
            => new(action, options);

        public static TransformManyBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, IEnumerable<TOutput>> action, int maxDegreeOfParallelism)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism });

        public static TransformManyBlock<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, Task<IEnumerable<TOutput>>> action, int maxDegreeOfParallelism)
            => new(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism });
    }

    [PublicAPI]
    public static class BlockObservableExtensions
    {
        public static IObservable<TResult> RecreateOnError<TInput, TResult>(this IObservable<TInput> observable, Func<IObservable<TInput>, IObservable<TResult>> factory, 
            Func<Exception, bool> canRecreate)
            => Observable.Create<TResult>(o =>
                                          {
                                              var dispo = new SerialDisposable();
                                              ErrorHandler(null);
                                              return dispo;

                                              IDisposable CreateLink(Action<TResult> next, Action<Exception> error, Action compled)
                                                  => factory(observable).Subscribe(next, error, compled);

                                              void ErrorHandler(Exception? error)
                                              {
                                                  if (error == null || canRecreate(error))
                                                  {
                                                      dispo.Disposable = CreateLink(o.OnNext, ErrorHandler, o.OnCompleted);
                                                      return;
                                                  }
                                                  
                                                  o.OnError(error);
                                                  dispo.Dispose();
                                              }
                                          });
    }
}