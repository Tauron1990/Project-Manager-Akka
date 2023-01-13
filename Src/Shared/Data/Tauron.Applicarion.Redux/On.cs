namespace Tauron.Applicarion.Redux;

public sealed record On<TState>(MutateState<TState> Mutator, Type ActionType);