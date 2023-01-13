namespace Tauron.Application.Akka.Redux;

public sealed record DispatchedAction<TState>(TState State, object Action);