namespace Tauron.Application.Akka.Redux;

public sealed record ActionState<TState, TAction>(TState State, TAction Action);