using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ReactiveUI;
using Stl.Fusion;
using Tauron.TAkka;

namespace Tauron.Application.Blazor;

[PublicAPI]
public abstract class StatefulViewModel<TData> : ReactiveObject, IActivatableViewModel, IResourceHolder
{
    private readonly CompositeDisposable _disposable = new();
    
    public IState<TData> State { get; }

    public IObservable<TData> NextElement { get; }

    protected StatefulViewModel(IStateFactory stateFactory)
    {
        State = stateFactory.NewComputed<TData>(ConfigureState, (_, cancel) => ComputeState(cancel))
           .DisposeWith(_disposable);

        NextElement = State.ToObservable();
    }
    
    public virtual ViewModelActivator Activator { get; } = new();

    protected virtual void ConfigureState(State<TData>.Options options) { }
    
    protected abstract Task<TData> ComputeState(CancellationToken cancellationToken);
    void IDisposable.Dispose()
        => _disposable.Dispose();

    void IResourceHolder.AddResource(IDisposable res)
        => _disposable.Add(res);

    void IResourceHolder.RemoveResource(IDisposable res)
        => _disposable.Remove(res);
}