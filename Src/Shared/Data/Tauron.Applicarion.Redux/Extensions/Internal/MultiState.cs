using System.Collections.Immutable;

namespace Tauron.Applicarion.Redux.Extensions.Internal;


public sealed record StateData(Guid Id, object ActualState);

public sealed record MultiState(ImmutableDictionary<Guid, StateData> Collection)
{
    public TState GetState<TState>(Guid id)
        where TState : new()
    {
        if (Collection.TryGetValue(id, out var state) && state.ActualState is TState actual)
            return actual;

        return new TState();
    }

    public MultiState UpdateState<TState>(Guid id, TState state)
        where TState : class
    {
        return Collection.TryGetValue(id, out var data) 
            ? this with { Collection = Collection.SetItem(id, data with { ActualState = state }) } 
            : this with { Collection = Collection.SetItem(id, new StateData(id, state)) };
    }
}