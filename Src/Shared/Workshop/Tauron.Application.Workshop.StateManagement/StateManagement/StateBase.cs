using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public abstract class StateBase<TData> : ICanQuery<TData>
    where TData : class, IStateEntity
{
    private IExtendedDataSource<MutatingContext<TData>>? _source;

    protected StateBase(ExtendedMutatingEngine<MutatingContext<TData>> engine)
    {
        OnChange = engine.EventSource(c => c.Data);
    }

    public IEventSource<TData> OnChange { get; }

    void IGetSource<TData>.DataSource(IExtendedDataSource<MutatingContext<TData>> source)
    {
        _source = source;
    }

    public async Task<TData?> Query(IQuery query)
    {
        var source = _source;
        try
        {
            return source is null ? null : (await source.GetData(query).ConfigureAwait(false)).Data;
        }
        finally
        {
            if(source != null)
                await source.OnCompled(query).ConfigureAwait(false);
        }
    }
}