using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tauron.Application.Dataflow.Internal
{
    internal sealed class UsingSource<TResource, TData> : ISourceBlock<TData>
        where TResource : IDisposable
    {
        private readonly ResourceHolder _resource = new();
        private readonly TaskSource<TData> _source;

        public UsingSource(Func<TResource> resource, Func<TResource, ISourceBlock<TData>> factory)
            : this(() => Task.Run(resource), res => Task.Run(() => factory(res))) { }

        public UsingSource(Func<Task<TResource>> resource, Func<TResource, Task<ISourceBlock<TData>>> factory)
        {
            _source = new TaskSource<TData>(Task.Run(async () =>
                                                     {
                                                         var res = await resource();
                                                         _resource.Resource = res;
                                                         return await factory(res);
                                                     }));

            _source.Completion.ContinueWith(_ => _resource.Resource?.Dispose());
        }

        void IDataflowBlock.Complete() => _source.Complete();
        void IDataflowBlock.Fault(Exception exception) => _source.Fault(exception);
        Task IDataflowBlock.Completion => _source.Completion;
        TData? ISourceBlock<TData>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target, out bool messageConsumed) => _source.ConsumeMessage(messageHeader, target, out messageConsumed);
        IDisposable ISourceBlock<TData>.LinkTo(ITargetBlock<TData> target, DataflowLinkOptions linkOptions) => _source.LinkTo(target, linkOptions);
        void ISourceBlock<TData>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) => _source.ReleaseReservation(messageHeader, target);
        bool ISourceBlock<TData>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<TData> target) => _source.ReserveMessage(messageHeader, target);

        private class ResourceHolder
        {
            public TResource? Resource { get; set; }
        }
    }
}