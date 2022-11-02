using NRules.RuleModel;
using SpaceConqueror.States.Actors;

namespace SpaceConqueror.States.Rooms;

public sealed class PlayerRoomState : IState
{
    public string LastRoom { get; private set; } = string.Empty;

    public string CurrentRoom { get; private set; } = string.Empty;


    public void SetNewRoom(RoomMoveSuccessCommand newRoom, IPlayerParty playerParty)
    {
        LastRoom = CurrentRoom;
        CurrentRoom = newRoom.Name;

        playerParty.PlayerPosition = newRoom.Name;
    }
}