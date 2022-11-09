using NRules.Fluent.Dsl;
using NRules.RuleModel;
using SpaceConqueror.States.Actors;
using SpaceConqueror.States.Rooms;

namespace SpaceConqueror.Rules.Rooms;

public class RoomHistoryUpdater : Rule, IRoomManager<RoomHistoryUpdater>
{
    public override void Define()
    {
        RoomMoveFailedCommand? failed = null;
        RoomMoveSuccessCommand? success = null;
        PlayerRoomState playerRoomState = null!;
        IPlayerParty playerParty = null!;

        When()
           .Match(() => playerRoomState)
           .Match(() => playerParty)
           .Or(
                exp => exp
                   .Match(() => failed)
                   .Match(() => success));

        Then()
           .Do(_ => ApplySucess(success, playerRoomState, playerParty))
           .Do(ctx => ApplyFail(ctx, failed));
    }

    private void ApplyFail(IContext ctx, RoomMoveFailedCommand? failed)
    {
        if(failed is null) return;

        ctx.InsertLinked(failed, new MoveToRoomCommand(GameManager.FailRoom, failed));
    }

    private void ApplySucess(RoomMoveSuccessCommand? success, PlayerRoomState playerRoomState, IPlayerParty playerParty)
    {
        if(success is null) return;

        playerRoomState.SetNewRoom(success, playerParty);
    }
}