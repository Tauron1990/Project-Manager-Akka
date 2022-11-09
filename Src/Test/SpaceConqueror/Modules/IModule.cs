namespace SpaceConqueror.Modules;

public interface IModule
{
    string Name { get; }

    string ModName { get; }

    Version Version { get; }

    ValueTask Initialize(ModuleConfiguration config, Action<string> messages);
}