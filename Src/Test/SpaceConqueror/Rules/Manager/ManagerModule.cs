using JetBrains.Annotations;
using SpaceConqueror.Core;
using SpaceConqueror.Modules;
using SpaceConqueror.Rules.Rooms;
using SpaceConqueror.States;
using SpaceConqueror.States.Actors;
using SpaceConqueror.States.GameTime;
using SpaceConqueror.States.Rooms;

namespace SpaceConqueror.Rules.Manager;

[UsedImplicitly]
public sealed class ManagerModule : ModuleBase
{
    public override string Name => "Processors";
    
    public override ValueTask Initialize(ModuleConfiguration config, Action<string> messages)
    {
        messages("Systeme werden Geladen");

        config.ManagerRegistrar.RegisterManager<GameTime, GameTimeProcessor, GameTimeInitializer>();
        config.ManagerRegistrar.RegisterManager<CommandProcessorState, CommandProcessor>();

        config.State.Add<PlayerRoomState>();
        config.Rules.Register<RoomMover>();
        config.Rules.Register<RoomHistoryUpdater>();
        config.Rules.Register<RoomUpdater>();

        config.Roonms.Register(GameManager.FailRoom);
        
        return default;
    }
}