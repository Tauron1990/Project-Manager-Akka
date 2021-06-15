using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    public sealed class StartUpBlock<TData> : ITargetBlock<TData>, ISourceBlock<TData>
    {
        private readonly BufferBlock<TData> _bufferBlock = new();

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TData messageValue, ISourceBlock<TData>? source, bool consumeToAccept) 
            => ((ITargetBlock<TData>)_bufferBlock).OfferMessage(messageHeader, messageValue, source, consumeToAccept);

        public void Complete() 
            => _bufferBlock.Complete();

        public void Fault(Exception exception)
            => ((IDataflowBlock) _bufferBlock).Fault(exception);

        public Task Completion => _bufferBlock.Completion;

        public TData? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed) 
            => ((ISourceBlock<TData>)_bufferBlock).ConsumeMessage(messageHeader, target, out messageConsumed);

        public IDisposable LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions) 
            => _bufferBlock.LinkTo(target, linkOptions);

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) 
            => ((ISourceBlock<TData>)_bufferBlock).ReleaseReservation(messageHeader, target);

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) 
            => ((ISourceBlock<TData>)_bufferBlock).ReserveMessage(messageHeader, target);
    }
}