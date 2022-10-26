using JetBrains.Annotations;
using SpaceConqueror.Modules;

namespace SpaceConqueror.Rules.Rooms;

[UsedImplicitly]
public sealed class Test : IModule
{
    public string Name => "Test";
    public string ModName => "Main";
    public Version Version => GameManager.GameVersion;

    public ValueTask Initialize(ModuleConfiguration config, Action<string> messages)
        => default;
}