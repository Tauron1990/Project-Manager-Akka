namespace Tauron.TextAdventure.Engine.Systems.Rooms;

public sealed class MoveToRommCommand : IGameCommand
{
    public MoveToRommCommand(string roomName)
        => RoomName = roomName;

    public string RoomName { get; }
}