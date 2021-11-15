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
public abstract class StatefulViewModel<TData> : ReactiveObject, IActivatableViewModel, IResourceHolder, IParameterUpdateable
{
    private readonly IStateFactory _stateFactory;
    private readonly CompositeDisposable _disposable = new();
    
    public IState<TData> State { get; }

    public IObservable<TData> NextElement { get; }

    protected StatefulViewModel(IStateFactory stateFactory)
    {
        _stateFactory = stateFactory;
        State = stateFactory.NewComputed<TData>(ConfigureState, (_, cancel) => ComputeState(cancel))
           .DisposeWith(_disposable);

        NextElement = State.ToObservable();
    }
    
    public virtual ViewModelActivator Activator { get; } = new();

    protected virtual void ConfigureState(State<TData>.Options options) { }
    
    protected abstract Task<TData> ComputeState(CancellationToken cancellationToken);

    public IState<TValue> GetParameter<TValue>(string name)
        => Updater.Register<TValue>(name, _stateFactory);

    void IDisposable.Dispose()
        => _disposable.Dispose();

    void IResourceHolder.AddResource(IDisposable res)
        => _disposable.Add(res);

    void IResourceHolder.RemoveResource(IDisposable res)
        => _disposable.Remove(res);

    public ParameterUpdater Updater { get; } = new();
}