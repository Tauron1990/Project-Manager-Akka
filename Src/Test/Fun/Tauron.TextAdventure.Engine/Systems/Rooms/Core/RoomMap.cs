namespace Tauron.TextAdventure.Engine.Systems.Rooms.Core;

internal sealed class RoomMap
{
    private readonly Dictionary<string, BaseRoom> _rooms = new();

    internal BaseRoom Get(string name)
        => _rooms[name];

    public void Add(string name, BaseRoom baseRoom)
        => _rooms[name] = baseRoom;
}