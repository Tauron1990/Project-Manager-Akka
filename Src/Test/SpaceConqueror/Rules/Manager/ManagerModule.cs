using JetBrains.Annotations;
using SpaceConqueror.Core;
using SpaceConqueror.Modules;
using SpaceConqueror.States;
using SpaceConqueror.States.GameTime;

namespace SpaceConqueror.Rules.Manager;

[UsedImplicitly]
public sealed class ManagerModule : ModuleBase
{
    public override string Name => "Processors";
    
    public override ValueTask Initialize(ModuleConfiguration config, Action<string> messages)
    {
        messages("Systeme werden Geladen");
        
        config.ManagerRegistrar.RegisterManager<GameTime, GameTimeProcessor>();
        config.ManagerRegistrar.RegisterManager<CommandProcessorState, CommandProcessor>();
        
        return default;
    }
}