namespace Tauron.Application.Akka.Redux.Extensions;

public sealed record MutateCallback<TState>(Func<TState, TState> Mutator);

public static class MutateCallback
{
    public static MutateCallback<TState> Create<TState>(Func<TState, TState> mutator)
        => new(mutator);
    
    public static MutateCallback<TState> Create<TState>(Func<TState> mutator)
        => new(_ => mutator());

    public static MutateCallback<TState> Create<TState>(TState state) 
        => new(_ => state);
}