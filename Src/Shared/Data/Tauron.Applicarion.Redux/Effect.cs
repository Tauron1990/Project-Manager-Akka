namespace Tauron.Applicarion.Redux;

public delegate IObservable<object> EffectFactory<TState>(IStore<TState> store);

public sealed record Effect<TState>(EffectFactory<TState> CreateEffect);