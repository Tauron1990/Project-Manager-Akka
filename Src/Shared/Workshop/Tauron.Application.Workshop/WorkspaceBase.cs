using JetBrains.Annotations;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop;

[PublicAPI]
public abstract class WorkspaceBase<TData> : IDataSource<TData> //, IState
    where TData : class
{
    protected WorkspaceBase(IDriverFactory driverFactory)
        => Engine = MutatingEngine.From(this, driverFactory);

    protected MutatingEngine<TData> Engine { get; }

    TData IDataSource<TData>.GetData() => GetDataInternal();

    void IDataSource<TData>.SetData(TData data)
        => SetDataInternal(data);

    public void Dispatch(IDataMutation mutationOld)
        => Engine.Mutate(mutationOld);

    protected abstract TData GetDataInternal();

    protected abstract void SetDataInternal(TData data);
}