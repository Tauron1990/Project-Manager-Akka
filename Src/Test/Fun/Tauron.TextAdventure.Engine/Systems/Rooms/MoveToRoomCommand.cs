namespace Tauron.TextAdventure.Engine.Systems.Rooms;

public sealed class MoveToRoomCommand : IGameCommand
{
    public MoveToRoomCommand(string roomName)
        => RoomName = roomName;

    public string RoomName { get; }
}