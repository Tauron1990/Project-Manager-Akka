using System;
using System.Threading.Tasks;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public sealed class MutationDataSource<TData> : IExtendedDataSource<MutatingContext<TData>>, IDisposable
        where TData : class, IStateEntity
    {
        private readonly IExtendedDataSource<TData> _original;

        public MutationDataSource(IExtendedDataSource<TData> original) => _original = original;

        public void Dispose()
        {
            if (_original is IDisposable source)
                source.Dispose();
        }

        public async Task<MutatingContext<TData>> GetData(IQuery query)
            => MutatingContext<TData>.New(await _original.GetData(query));

        public async Task SetData(IQuery query, MutatingContext<TData> data)
        {
            var (_, entity) = data;

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (entity is IChangeTrackable { IsChanged: false }) return;

            await _original.SetData(query, entity);
        }

        public Task OnCompled(IQuery query) => _original.OnCompled(query);
    }
}