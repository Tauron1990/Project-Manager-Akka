using NRules.Fluent.Dsl;
using NRules.RuleModel;
using SpaceConqueror.Core;
using SpaceConqueror.States.Rooms;

namespace SpaceConqueror.Rules.Rooms;

public sealed class RoomUpdater : Rule, IRoomManager<RoomUpdater>
{
    public override void Define()
    {
        RoomMoveSuccessCommand successCommand = null!;
        RoomState old = null!;
        RoomState newState = null!;

        AssetManager assetManager = null!;


        Dependency().Resolve(() => assetManager);
        
        When()
           .Match(() => successCommand)
           .Match(() => old, o => o.Id == successCommand.From)
           .Match(() => newState, o => o.Id == successCommand.Name);

        Then().Do(ctx => UpdateRooms(ctx, old, newState, assetManager, successCommand));
    }

    private void UpdateRooms(IContext ctx, RoomState old, RoomState newState, AssetManager assetManager, RoomMoveSuccessCommand command)
    {
        old.IsPlayerInRoom = false;
        newState.IsPlayerInRoom = true;
        
        ctx.InsertAll(newState.GetToInsert(assetManager, command.Context));
    }
}