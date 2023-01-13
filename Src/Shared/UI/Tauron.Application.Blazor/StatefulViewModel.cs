using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Stl.Fusion;

namespace Tauron.Application.Blazor;

[PublicAPI]
public abstract class StatefulViewModel<TData> : BlazorViewModel
{
    protected StatefulViewModel(IStateFactory stateFactory)
        : base(stateFactory)
    {
        var ops = new ComputedState<TData>.Options();
        // ReSharper disable once VirtualMemberCallInConstructor
        #pragma warning disable MA0056
        ConfigureState(ops);
        #pragma warning restore MA0056
        State = stateFactory.NewComputed(ops, (_, cancel) => ComputeState(cancel))
           .DisposeWith(this);

        NextElement = State.ToObservable(_ => true);
    }

    public IState<TData> State { get; }

    public IObservable<TData> NextElement { get; }

    protected virtual void ConfigureState(ComputedState<TData>.Options options) { }

    protected abstract Task<TData> ComputeState(CancellationToken cancellationToken);
}