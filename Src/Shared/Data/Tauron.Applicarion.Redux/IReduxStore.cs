﻿using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public interface IReduxStore<TState> : IActionDispatcher, IDisposable
{
    TState CurrentState { get; }

    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);

    IObservable<TState> Select();

    IObservable<ActionState<TState, TAction>> ObservActionState<TAction>();

    IObservable<TResult> ObservAction<TAction, TResult>(Func<TState, TAction, TResult> selector);

    void Reset();

    void RegisterReducers(IEnumerable<On<TState>> reducers);

    void RegisterEffects(IEnumerable<Effect<TState>> effects);


    void RegisterReducers(params On<TState>[] reducers);

    void RegisterEffects(params Effect<TState>[] effects);

    IObservable<object> ObserveAction();

    IObservable<TAction> ObserveAction<TAction>()
        where TAction : class;

    IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector)
        where TAction : class;
}