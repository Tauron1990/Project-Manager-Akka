using NRules.Fluent.Dsl;

namespace SpaceConqueror.Rules.Rooms;

public interface IRoomManager<TManager>
    where TManager : Rule, IRoomManager<TManager> { }