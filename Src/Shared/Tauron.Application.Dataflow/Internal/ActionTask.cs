using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal static class ActionTask
    {
        public static readonly int ThreadCount = Environment.ProcessorCount * 2;
    }

    internal sealed class ActionTask<TData>
    {
        private int _count;

        public Task OnFinish { get; }

        public ActionTask(ISourceBlock<TData> source, Action<TData> onNext, CancellationToken token)
            : this(source, (d, _) => onNext(d), token) { }

        public ActionTask(ISourceBlock<TData> source, Action<TData, int> onNext, CancellationToken token)
        {
            var block = ActionBlock.Create<TData>(ele =>
                                                  {
                                                      onNext(ele, Interlocked.Increment(ref _count));
                                                  }, ActionTask.ThreadCount, ActionTask.ThreadCount);

            var link = source.LinkTo(block, SourceBlockExtensions.DefaultAnonymosLinkOptions);

            if (token.CanBeCanceled) 
                token.Register(() =>
                               {
                                   link.Dispose();
                                   block.Complete();
                               });

            OnFinish = block.Completion;
        }

        public static implicit operator Task(ActionTask<TData> task)
            => task.OnFinish;
    }
}