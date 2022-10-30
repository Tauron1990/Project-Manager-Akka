namespace SpaceConqueror.States.Rooms;

public sealed record MoveToRoomCommand(string Name) : IGameCommand;