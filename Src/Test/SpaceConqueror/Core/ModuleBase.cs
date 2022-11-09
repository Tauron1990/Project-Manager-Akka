using SpaceConqueror.Modules;

namespace SpaceConqueror.Core;

public abstract class ModuleBase : IModule
{
    public abstract string Name { get; }

    public string ModName => "Main App";

    public Version Version => GameManager.GameVersion;

    public abstract ValueTask Initialize(ModuleConfiguration config, Action<string> messages);
}