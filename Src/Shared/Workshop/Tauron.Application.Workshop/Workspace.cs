using System.Reactive.Linq;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Analyzing;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutating.Changes;
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

[PublicAPI]
public abstract class Workspace<TThis, TRawData> : WorkspaceBase<MutatingContext<TRawData>>
    where TThis : Workspace<TThis, TRawData>

{
    protected Workspace(IDriverFactory factory)
        : base(factory)
        => Analyzer = Analyzing.Analyzer.From<TThis, MutatingContext<TRawData>>((TThis)this, factory);

    public IAnalyzer<TThis, MutatingContext<TRawData>> Analyzer { get; }

    public void Reset(TRawData newData)
        => Engine.Mutate(nameof(Reset), data => data.Select(d => d.Update(new ResetChange(), newData)));
}