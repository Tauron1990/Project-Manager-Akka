using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal abstract class PerlinkSource<TData, TBlock> : ISourceBlock<TData>
        where TBlock : ISourceBlock<TData>
    {
        protected abstract void NextBlock(TBlock block);

        protected abstract TBlock NewBlock();

        void IDataflowBlock.Complete() => throw new NotSupportedException();

        void IDataflowBlock.Fault(Exception exception) => throw new NotSupportedException();

        Task IDataflowBlock.Completion => throw new NotSupportedException();

        TData? ISourceBlock<TData>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed) => throw new NotSupportedException();

        IDisposable ISourceBlock<TData>.LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions)
        {
            var block = NewBlock();
            var dispo = block.LinkTo(target, linkOptions);
            NextBlock(block);
            return dispo;
        }

        void ISourceBlock<TData>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) => throw new NotSupportedException();

        bool ISourceBlock<TData>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) => throw new NotSupportedException();
    }
}