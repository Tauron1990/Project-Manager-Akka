using NRules.Fluent.Dsl;
using NRules.RuleModel;
using SpaceConqueror.States.Rooms;

namespace SpaceConqueror.Rules.Rooms;

public class RoomHistoryUpdater : Rule, IRoomManager<RoomHistoryUpdater>
{
    public override void Define()
    {
        RoomMoveFailedCommand? failed = null;
        RoomMoveSuccessCommand? success = null;
        PlayerRoomState playerRoomState = null!;

        When()
           .Match(() => playerRoomState)
           .Or(
                exp => exp
                   .Match(() => failed)
                   .Match(() => success));

        Then()
           .Do(_ => ApplySucess(success, playerRoomState))
           .Do(ctx => ApplyFail(ctx, failed));
    }

    private void ApplyFail(IContext ctx, RoomMoveFailedCommand? failed)
    {
        if(failed is null) return;
        
        ctx.InsertLinked(failed, new MoveToRoomCommand(GameManager.FailRoom, failed));
    }

    private void ApplySucess(RoomMoveSuccessCommand? success, PlayerRoomState playerRoomState)
    {
        if(success is null) return;
        
        playerRoomState.LastRoom = playerRoomState.CurrentRoom;
        playerRoomState.CurrentRoom = success.Name;
    }
}