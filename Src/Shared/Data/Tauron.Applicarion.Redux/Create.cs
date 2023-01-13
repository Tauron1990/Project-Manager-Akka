using System.Reactive.Concurrency;
using JetBrains.Annotations;
using Tauron.Applicarion.Redux.Internal;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public static class Create
{
    public static IReduxStore<TState> Store<TState>(TState initialState, Action<Exception> errors)
        => new Store<TState>(initialState, Scheduler.Default, errors);

    public static IReduxStore<TState> Store<TState>(TState initialState, IScheduler scheduler, Action<Exception> errors)
        => new Store<TState>(initialState, scheduler, errors);

    public static Effect<TState> Effect<TState>(Func<IObservable<object?>> effectFactory)
        => new(_ => effectFactory());

    public static Effect<TState> Effect<TState>(EffectFactory<TState> effectFactory)
        => new(effectFactory);

    public static On<TState> On<TAction, TState>(Func<TState, TState> mutator)
        => new((s, _) => mutator(s), typeof(TAction));

    public static On<TState> On<TAction, TState>(Func<TState, TAction, TState> mutator)
        => new((s, a) => a is TAction action ? mutator(s, action) : s, typeof(TAction));


    public static IStateLens<TState, TFeatureState> CreateSubReducers<TState, TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer)
        => new ExplicitStateLens<TState, TFeatureState>(featureSelector, stateReducer);
}