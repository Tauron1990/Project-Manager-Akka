namespace Tauron.Applicarion.Redux;

public delegate TState MutateState<TState>(TState state, object action);

public sealed record On<TState>(MutateState<TState> Mutator, Type ActionType);