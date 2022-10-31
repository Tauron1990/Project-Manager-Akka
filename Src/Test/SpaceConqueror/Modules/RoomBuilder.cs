using JetBrains.Annotations;
using SpaceConqueror.Core;
using SpaceConqueror.States;
using SpaceConqueror.States.Rendering;
using SpaceConqueror.States.Rooms;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace SpaceConqueror.Modules;

[PublicAPI]
public class RoomBuilder
{
    private readonly AssetManager _assetManager;
    internal List<Type> Rules { get; } = new();
    internal StateRegistrar States { get; } = new();

    private readonly RoomState _roomState;

    internal RoomBuilder(string name, AssetManager assetManager)
    {
        _assetManager = assetManager;
        _roomState = new RoomState(name);
    }

    internal (IEnumerable<Type> Rules, Func<IEnumerable<IState>> States) Init()
        => (Rules.AsEnumerable(), GetStates);

    private IEnumerable<IState> GetStates()
    {
        yield return _roomState;

        foreach (IState states in States.GetStates()())
            yield return states;
    }

    public RoomBuilder ExcludeFromHistory()
    {
        _roomState.ExcludeHistory = true;

        return this;
    }

    public RoomBuilder AddDescription(string name)
    {
        _roomState.PredefinedDescriptions.Add(_roomState.PredefinedDescriptions.Count + 1, name);
        
        return this;
    }
    
    public RoomBuilder AddDescription(string name, string value)
    {
        name = $"{_roomState.Id}_{name}";
        
        AddDescription(name);

        static Func<string> Data(string value)
            => () => value;

        _assetManager.Add(name, Data(value));
        
        return this;
    }

    public RoomBuilder AddCommand(IDisplayCommand command)
    {
        var id = $"{_roomState.Id}_{command.Name}";
        
        _assetManager.Add(id, () => command);
        _roomState.PredefinedCommands.Add(id);

        return this;
    }

    public RoomBuilder ProcessContext<TContext>(Func<TContext, string> descriptor)
        => ProcessContext<TContext>(t => new Markup(descriptor(t)));

    public RoomBuilder ProcessContext<TContext>(Func<TContext, IRenderable> descriptor)
        => ProcessContext<TContext>(t => new StaticDisplayData(int.MaxValue, descriptor(t)));
    
    public RoomBuilder ProcessContext<TContext>(Func<TContext, IDisplayData> descriptor);
    
    public RoomBuilder ProcessContext<TContext>(Func<TContext, IGameCommand> commands);
    
    public RoomBuilder ProcessContext<TContext>(Func<TContext, IDisplayCommand> commands);
}