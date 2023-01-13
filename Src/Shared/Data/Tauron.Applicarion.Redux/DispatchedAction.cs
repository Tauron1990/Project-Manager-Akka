namespace Tauron.Applicarion.Redux;

public sealed record DispatchedAction<TState>(TState State, object? Action);