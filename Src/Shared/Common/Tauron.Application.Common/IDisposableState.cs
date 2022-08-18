using System;
using System.Diagnostics.CodeAnalysis;
using Stl;
using Stl.Conversion;
using Stl.Fusion;

namespace Tauron;

public interface IDisposableState<TData> : IState<TData>, IDisposable
{
    
}

public sealed class DisposableState<TData> : IDisposableState<TData>
{
    private readonly IState<TData> _state;
    private readonly IDisposable _subscription;

    public DisposableState(IMutableState<TData> state, IDisposable subscription)
    {
        _state = state;
        _subscription = subscription;
    }

    public Result<TOther> Cast<TOther>()
        => _state.Cast<TOther>();

    public bool HasValue => _state.HasValue;
    public object? UntypedValue => _state.UntypedValue;

    public TData Value => _state.Value;

    public void Deconstruct(out TData value, out Exception? error)
    {
        _state.Deconstruct(out value, out error);
    }

    public bool IsValue([MaybeNullWhen(false)]out TData value)
        => _state.IsValue(out value);

    #pragma warning disable CS8767
    public bool IsValue([MaybeNullWhen(false)]out TData value, [MaybeNullWhen(false)]out Exception error)
        #pragma warning restore CS8767
        => _state.IsValue(out value!, out error!);

    public Result<TData> AsResult()
        => _state.AsResult();

    public TData? ValueOrDefault => _state.ValueOrDefault;

    object? IResult.UntypedValue => _state.UntypedValue;

    public bool HasError => _state.HasError;

    public Exception? Error => _state.Error;

    public IServiceProvider Services => _state.Services;

    IStateSnapshot IState.Snapshot => ((IState)_state).Snapshot;

    public IComputed<TData> Computed => _state.Computed;

    public TData LatestNonErrorValue => _state.LatestNonErrorValue;

    event Action<IState<TData>, StateEventKind>? IState<TData>.Invalidated
    {
        add => _state.Invalidated += value;
        remove => _state.Invalidated -= value;
    }

    event Action<IState<TData>, StateEventKind>? IState<TData>.Updating
    {
        add => _state.Updating += value;
        remove => _state.Updating -= value;
    }

    event Action<IState<TData>, StateEventKind>? IState<TData>.Updated
    {
        add => _state.Updated += value;
        remove => _state.Updated -= value;
    }

    public StateSnapshot<TData> Snapshot => _state.Snapshot;

    IComputed IState.Computed => ((IState)_state).Computed;

    object? IState.LatestNonErrorValue => ((IState)_state).LatestNonErrorValue;

    event Action<IState, StateEventKind>? IState.Invalidated
    {
        add => ((IState)_state).Invalidated += value;
        remove => ((IState)_state).Invalidated -= value;
    }

    event Action<IState, StateEventKind>? IState.Updating
    {
        add => ((IState)_state).Updating += value;
        remove => ((IState)_state).Updating -= value;
    }

    event Action<IState, StateEventKind>? IState.Updated
    {
        add => ((IState)_state).Updated += value;
        remove => ((IState)_state).Updated -= value;
    }

    TData IConvertibleTo<TData>.Convert()
        => ((IConvertibleTo<TData>)_state).Convert();

    Result<TData> IConvertibleTo<Result<TData>>.Convert()
        => ((IConvertibleTo<Result<TData>>)_state).Convert();

    public void Dispose()
    {
        _subscription.Dispose();
    }
}