using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

    }
}