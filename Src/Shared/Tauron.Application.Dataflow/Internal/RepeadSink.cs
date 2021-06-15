using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class RepeatSource<TData> : PerlinkSource<TData, BufferBlock<TData>>
    {
        private readonly TData _data;
        private readonly int _count;

        public RepeatSource(TData data, int count)
        {
            _data = data;
            _count = count;
        }

        public async Task Fill(ITargetBlock<TData> buffer, TData data, int count)
        {
            try
            {
                for (var i = 0; i < count; i++) 
                    await buffer.SendAsync(data);
                buffer.Complete();
            }
            catch (Exception e)
            {
                buffer.Fault(e);
            }
        }

        protected override void NextBlock(BufferBlock<TData> block) 
            => Task.Run(() => Fill(block, _data, _count));

        protected override BufferBlock<TData> NewBlock() => BufferBlock.Create<TData>();
    }
}