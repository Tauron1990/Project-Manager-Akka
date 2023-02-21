using System;
using System.Reactive.Disposables;
using JetBrains.Annotations;
using ReactiveUI;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;

namespace Tauron.Application.Blazor;

[PublicAPI]
public class BlazorViewModel : ReactiveObject, IActivatableViewModel, IResourceHolder, IParameterUpdateable
{
    private readonly CompositeDisposable _disposable = new();

    protected BlazorViewModel(IStateFactory stateFactory)
        => StateFactory = stateFactory;

    public IStateFactory StateFactory { get; }

    public ViewModelActivator Activator { get; } = new();

    public ParameterUpdater Updater { get; } = new();

    void IDisposable.Dispose()
        => _disposable.Dispose();

    void IResourceHolder.AddResource(IDisposable res)
        => _disposable.Add(res);

    void IResourceHolder.RemoveResource(IDisposable res)
        => _disposable.Remove(res);


    public IState<TValue> GetParameter<TValue>(string name)
        => this.GetParameter<TValue>(name, StateFactory);
}