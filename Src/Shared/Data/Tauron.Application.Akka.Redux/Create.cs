using System.Reactive.Concurrency;
using System.Runtime.InteropServices.ComTypes;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using JetBrains.Annotations;
using Tauron.Application.Akka.Redux.Internal;

namespace Tauron.Application.Akka.Redux;

[PublicAPI]
public static class Create
{
    public static IReduxStore<TState> Store<TState>(TState initialState, IMaterializer materializer, Action<Exception> errors)
        => new Store<TState>(initialState, errors, materializer);

    public static Effect<TState> Effect<TState>(Func<Source<object, NotUsed>> effectFactory) 
        => new(_ => effectFactory());

    public static Effect<TState> Effect<TState>(EffectFactory<TState> effectFactory)
        => new(effectFactory);

    public static On<TState> On<TAction, TState>(Flow<TState, TState, NotUsed> reducer) 
        => new(
            Flow.Create<DispatchedAction<TState>>()
           .Select(da => da.State)
           .Via(reducer),
            typeof(TAction));

    public static On<TState> On<TAction, TState>(Flow<(TState State, TAction Action), TState, NotUsed> reducer)
        => new(
            Flow.Create<DispatchedAction<TState>>()
               .Where(i => i.Action is TAction)
               .Select(da => (da.State, (TAction)da.Action!))
               .Via(reducer)
            , typeof(TAction));
    
    
    public static IStateLens<TState, TFeatureState> CreateSubReducers<TState, TFeatureState>(Func<TState, TFeatureState> featureSelector, Func<TState, TFeatureState, TState> stateReducer, IMaterializer materializer)
        => new ExplicitStateLens<TState, TFeatureState>(featureSelector, stateReducer, materializer);
}