namespace Tauron.Applicarion.Redux;

public delegate TState MutateState<TState>(TState state, object? action);