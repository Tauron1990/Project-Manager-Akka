namespace SpaceConqueror.Modules;

public interface IModule
{
    string Name { get; }

    string Category { get; }

    Version Version { get; }
    
    ValueTask Initialize(ModuleConfiguration config);
}