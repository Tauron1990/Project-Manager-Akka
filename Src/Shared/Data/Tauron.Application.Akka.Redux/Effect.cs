namespace Tauron.Application.Akka.Redux;

public sealed record Effect<TState>(EffectFactory<TState> CreateEffect);