namespace Tauron.Applicarion.Redux;

public sealed record Effect<TState>(EffectFactory<TState> CreateEffect);