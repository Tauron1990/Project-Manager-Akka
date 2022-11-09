using NRules.Fluent.Dsl;
using SpaceConqueror.States.Rooms;

namespace SpaceConqueror.Rules.Rooms;

public class RoomMover : Rule, IRoomManager<RoomMover>
{
    public override void Define()
    {
        MoveToRoomCommand command = null!;
        PlayerRoomState playerRoomState = null!;
        IEnumerable<RoomState> rooms = null!;

        When()
           .Match(() => command)
           .Match(() => playerRoomState)
           .Query(
                () => rooms,
                x => x
                   .Match<RoomState>(s => s.Id == command.Name)
                   .Collect());

        Then().Yield(_ => SelectResultCommand(command, rooms, playerRoomState));
    }

    private object SelectResultCommand(MoveToRoomCommand command, IEnumerable<RoomState> rooms, PlayerRoomState playerRoomState)
    {
        RoomState? room = rooms.SingleOrDefault();

        if(room is not null)
            return new RoomMoveSuccessCommand(command.Name, playerRoomState.CurrentRoom, room.ExcludeHistory, command.Context);

        if(command.Name == GameManager.FailRoom)
            throw new InvalidOperationException("Fail Room not Found");

        return new RoomMoveFailedCommand(command.Name);

    }
}