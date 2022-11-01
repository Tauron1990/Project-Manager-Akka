using SpaceConqueror.Core;
using SpaceConqueror.States.Rendering;

namespace SpaceConqueror.States.Rooms;

public delegate IEnumerable<object?> ContextProcessor(AssetManager assetManager, object context);

public enum RoomType
{
    Conventinal,
    Dungeon,
    Special
}

public class RoomState : IState
{
    public string Id { get; }

    public RoomType RoomType { get; set; }
    
    public bool IsPlayerInRoom { get; set; }
    
    public bool ExcludeHistory { get; set; }

    public Dictionary<int, string> PredefinedDescriptions { get; } = new();

    public List<string> PredefinedCommands { get; } = new();

    public List<string> ProcessorNames { get; } = new();

    public RoomState(string id)
        => Id = id;

    public IEnumerable<object> GetToInsert(AssetManager assetManager, object context)
    {
        foreach (var description in PredefinedDescriptions)
            yield return new AssetDisplayData(description.Key, assetManager, description.Value);

        foreach (string command in PredefinedCommands)
            yield return assetManager.Get<IDisplayCommand>(command);

        foreach (object? processor in from processorName in ProcessorNames
                                     from element in assetManager.Get<ContextProcessor>(processorName)(assetManager, context)
                                     where element is not null
                                     select element)
            yield return processor;
    }
}