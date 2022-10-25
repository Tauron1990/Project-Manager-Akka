using System.Collections.Immutable;

namespace SpaceConqueror.Modules;

public sealed class ModuleManager
{
    public ImmutableList<RegistratedModuleCategory> Modules { get; private set; } = ImmutableList<RegistratedModuleCategory>.Empty;

    public static ValueTask LoadModules(string path, ModuleConfiguration configuration, Action<string> messeages)
    {
        
    }
}