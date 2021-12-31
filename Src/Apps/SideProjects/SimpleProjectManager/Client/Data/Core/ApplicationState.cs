using System.Collections.Immutable;
using ReduxSimple.Entity;

namespace SimpleProjectManager.Client.Data.Core;


public sealed record StateData(Guid Id, object ActualState);

public sealed record ApplicationState : EntityState<Guid, StateData>
{
    public static EntityAdapter<Guid, StateData> Adapter { get; } = EntityAdapter<Guid, StateData>.Create(d => d.Id);

    public TState GetState<TState>(Guid id)
        where TState : new()
    {
        if (Collection.TryGetValue(id, out var state) && state.ActualState is TState actual)
            return actual;

        return new TState();
    }

    public ApplicationState UpdateState<TState>(Guid id, TState state)
        where TState : class
    { 
        return Adapter.UpsertOne(new StateData(id, state), this);
    }
}