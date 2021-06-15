using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Akka.Util;

namespace Tauron.Application.Dataflow.Internal
{
    public sealed class DeferSource<TData> : ISourceBlock<TData>
    {
        private readonly FastLazy<ISourceBlock<TData>> _block;
        private ISourceBlock<TData> ActualBlock => _block.Value;

        public DeferSource(Func<ISourceBlock<TData>> factory) 
            => _block = new FastLazy<ISourceBlock<TData>>(factory);

        public void Complete() => ActualBlock.Complete();

        public void Fault(Exception exception) => ActualBlock.Fault(exception);

        public Task Completion => ActualBlock.Completion;

        public TData? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed)
            => ActualBlock.ConsumeMessage(messageHeader, target, out messageConsumed);

        public IDisposable LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions) 
            => ActualBlock.LinkTo(target, linkOptions);

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target)
            => ActualBlock.ReleaseReservation(messageHeader, target);

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target)
            => ActualBlock.ReserveMessage(messageHeader, target);
    }

    public sealed class AsyncDeferSource<TData> : ISourceBlock<TData>
    {
    }
}