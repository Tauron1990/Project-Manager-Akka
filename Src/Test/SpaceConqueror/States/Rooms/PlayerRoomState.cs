namespace SpaceConqueror.States.Rooms;

public sealed class PlayerRoomState : IState
{
    public string LastRoom { get; set; } = string.Empty;

    public string CurrentRoom { get; set; } = string.Empty;
}