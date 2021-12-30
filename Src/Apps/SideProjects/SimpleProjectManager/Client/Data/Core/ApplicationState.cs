using System.Collections.Immutable;
using ReduxSimple.Entity;

namespace SimpleProjectManager.Client.Data.Core;

public sealed record Request(Guid Id, object RequestData);

public sealed record RequestState : EntityState<Guid, Request>
{
    public static EntityAdapter<Guid, Request> Adapter { get; } = EntityAdapter<Guid, Request>.Create(r => r.Id);
}

public sealed record StateData(Guid Id, object ActualState, RequestState PendingRequests);

public sealed record ApplicationState : EntityState<Guid, StateData>
{
    public static EntityAdapter<Guid, StateData> Adapter { get; } = EntityAdapter<Guid, StateData>.Create(d => d.Id);
}