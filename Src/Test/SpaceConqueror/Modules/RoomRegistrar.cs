using System.Collections.Concurrent;

namespace SpaceConqueror.Modules;

public sealed class RoomRegistrar
{
    private readonly ConcurrentDictionary<string, RoomBuilder> _rules = new();
    private readonly StateRegistrar _stateRegistrar;

    public RoomRegistrar(StateRegistrar stateRegistrar)
        => _stateRegistrar = stateRegistrar;
}