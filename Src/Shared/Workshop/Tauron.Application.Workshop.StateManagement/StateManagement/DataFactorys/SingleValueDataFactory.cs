using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.DataFactorys;

[PublicAPI]
public abstract class SingleValueDataFactory<TData> : AdvancedDataSourceFactory
    where TData : IStateEntity
{
    private readonly object _lock = new();
    private SingleValueSource? _source;

    public override bool CanSupply(Type dataType) => dataType == typeof(TData);

    public override Func<IExtendedDataSource<TRealData>> Create<TRealData>(CreationMetadata? metadata)
    {
        return () =>
               {
                   if(_source != null) return (IExtendedDataSource<TRealData>)(object)_source;

                   lock (_lock)
                   {
                       switch (_source)
                       {
                           case null:
                               _source = new SingleValueSource(CreateValue(metadata));

                               return (IExtendedDataSource<TRealData>)(object)_source;
                           default:
                               return (IExtendedDataSource<TRealData>)(object)_source;
                       }
                   }
               };
    }

    protected abstract Task<TData> CreateValue(CreationMetadata? metadata);

    private sealed class SingleValueSource : IExtendedDataSource<TData>, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new(0, 1);
        private Task<TData> _value;

        internal SingleValueSource(Task<TData> value) => _value = value;

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