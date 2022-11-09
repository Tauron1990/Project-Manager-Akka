using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ReactiveUI;
using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;

namespace Tauron.Application.Blazor;

public static class BlazorViewModelExtensions
{
    public static IState<TValue> GetParameter<TValue>(this IParameterUpdateable updateable, string name, IStateFactory stateFactory)
        => updateable.Updater.Register<TValue>(name, stateFactory);
}

[PublicAPI]
public class BlazorViewModel : ReactiveObject, IActivatableViewModel, IResourceHolder, IParameterUpdateable
{
    private readonly CompositeDisposable _disposable = new();

    protected BlazorViewModel(IStateFactory stateFactory)
        => StateFactory = stateFactory;

    public IStateFactory StateFactory { get; }

    public virtual ViewModelActivator Activator { get; } = new();

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

[PublicAPI]
public abstract class StatefulViewModel<TData> : BlazorViewModel
{
    protected StatefulViewModel(IStateFactory stateFactory)
        : base(stateFactory)
    {
        var ops = new ComputedState<TData>.Options();
        // ReSharper disable once VirtualMemberCallInConstructor
        ConfigureState(ops);
        State = stateFactory.NewComputed(ops, (_, cancel) => ComputeState(cancel))
           .DisposeWith(this);

        NextElement = State.ToObservable(_ => true);
    }

    public IState<TData> State { get; }

    public IObservable<TData> NextElement { get; }

    protected virtual void ConfigureState(ComputedState<TData>.Options options) { }

    protected abstract Task<TData> ComputeState(CancellationToken cancellationToken);
}