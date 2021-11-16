using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ReactiveUI;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;
using Tauron.TAkka;

namespace Tauron.Application.Blazor;

[PublicAPI]
public class BlazorViewModel : ReactiveObject, IActivatableViewModel, IResourceHolder, IParameterUpdateable
{
    public IStateFactory StateFactory { get; }
    private readonly CompositeDisposable _disposable = new();

    protected BlazorViewModel(IStateFactory stateFactory)
        => StateFactory = stateFactory;

    public virtual ViewModelActivator Activator { get; } = new();
    

    public IState<TValue> GetParameter<TValue>(string name)
        => Updater.Register<TValue>(name, StateFactory);

    void IDisposable.Dispose()
        => _disposable.Dispose();

    void IResourceHolder.AddResource(IDisposable res)
        => _disposable.Add(res);

    void IResourceHolder.RemoveResource(IDisposable res)
        => _disposable.Remove(res);

    public ParameterUpdater Updater { get; } = new();
}

[PublicAPI]
public abstract class StatefulViewModel<TData> : BlazorViewModel
{
    public IState<TData> State { get; }

    public IObservable<TData> NextElement { get; }

    protected StatefulViewModel(IStateFactory stateFactory)
        : base(stateFactory)
    {
        State = stateFactory.NewComputed<TData>(ConfigureState, (_, cancel) => ComputeState(cancel))
           .DisposeWith(this);

        NextElement = State.ToObservable();
    }
    
    protected virtual void ConfigureState(State<TData>.Options options) { }
    
    protected abstract Task<TData> ComputeState(CancellationToken cancellationToken);
}