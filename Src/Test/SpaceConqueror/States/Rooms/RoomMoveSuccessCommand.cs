namespace SpaceConqueror.States.Rooms;

public sealed record RoomMoveSuccessCommand(string Name, string From, bool Exclude, object Context);

public sealed record RoomMoveFailedCommand(string Name);