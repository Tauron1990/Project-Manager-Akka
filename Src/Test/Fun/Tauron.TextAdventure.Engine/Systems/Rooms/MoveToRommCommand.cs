namespace Tauron.TextAdventure.Engine.Systems.Rooms;

public sealed class MoveToRommCommand : IGameCommand
{
    public string RoomName { get; }

    public MoveToRommCommand(string roomName)
        => RoomName = roomName;
}