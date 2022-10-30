using NRules.Fluent.Dsl;
using SpaceConqueror.States.Rooms;

namespace SpaceConqueror.Rules.Rooms;

public sealed class RoomUpdater : Rule, IRoomManager<RoomUpdater>
{
    public override void Define()
    {
        RoomMoveSuccessCommand successCommand = null!;
        RoomState old = null!;
        RoomState newState = null!;

        When()
           .Match(() => successCommand)
           .Match(() => old, o => o.Id == successCommand.From)
           .Match(() => newState, o => o.Id == successCommand.Name);

        Then().Do(_ => UpdateRooms(old, newState));
    }

    private void UpdateRooms(RoomState old, RoomState newState)
    {
        old.IsPlayerInRoom = false;
        newState.IsPlayerInRoom = true;
    }
}