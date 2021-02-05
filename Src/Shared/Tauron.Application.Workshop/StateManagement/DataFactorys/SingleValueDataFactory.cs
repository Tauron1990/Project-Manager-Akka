using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys
{
    [PublicAPI]
    public abstract class SingleValueDataFactory<TData> : AdvancedDataSourceFactory
        where TData : IStateEntity
    {
        private readonly Lazy<object> _lazyData;

        protected SingleValueDataFactory()
        {
            _lazyData = new Lazy<object>(() => new SingleValueSource(CreateValue()),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public override bool CanSupply(Type dataType) => dataType == typeof(TData);

        public override Func<IExtendedDataSource<TRealData>> Create<TRealData>()
        {
            return () => (IExtendedDataSource<TRealData>) _lazyData.Value;
        }

        protected abstract Task<TData> CreateValue();

        private sealed class SingleValueSource : IExtendedDataSource<TData>, IDisposable
        {
            private readonly SemaphoreSlim _semaphore = new(0, 1);
            private Task<TData> _value;

            public SingleValueSource(Task<TData> value) => _value = value;

            public void Dispose()
            {
                _semaphore.Dispose();
            }

            public async Task<TData> GetData(IQuery query)
            {
                await _semaphore.WaitAsync();
                return await _value;
            }

            public Task SetData(IQuery query, TData data)
            {
                _value = Task.FromResult(data);
                return Task.CompletedTask;
            }

            public Task OnCompled(IQuery query)
            {
                _semaphore.Release();
                return Task.CompletedTask;
            }
        }
    }
}