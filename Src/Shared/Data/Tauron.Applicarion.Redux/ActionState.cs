namespace Tauron.Applicarion.Redux;

public sealed record ActionState<TState, TAction>(TState State, TAction Action);