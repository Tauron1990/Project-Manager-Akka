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

    private readonly RoomState _roomState;

    internal RoomBuilder(string name, AssetManager assetManager)
    {
        _assetManager = assetManager;
        _roomState = new RoomState(name);
    }

    internal List<Type> Rules { get; } = new();
    internal StateRegistrar States { get; } = new();

    internal (IEnumerable<Type> Rules, Func<IEnumerable<IState>> States) Init()
        => (Rules.AsEnumerable(), GetStates);

    public RoomBuilder WithRoomType(RoomType roomType)
    {
        _roomState.RoomType = roomType;

        return this;
    }

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
            => () => GameManager.AssetManager.GetString(value);

        _assetManager.Add(name, Data(value));

        return this;
    }

    public RoomBuilder SetRoomType(RoomType roomType)
    {
        _roomState.RoomType = roomType;

        return this;
    }

    public RoomBuilder AddCommand(IDisplayCommand command)
    {
        var id = $"{_roomState.Id}_{command.Name}";

        _assetManager.Add(id, () => command);
        _roomState.PredefinedCommands.Add(id);

        return this;
    }

    public RoomBuilder ProcessToData<TContext>(string id, Func<TContext, AssetManager, string> descriptor)
        => ProcessToData<TContext>(id, (t, m) => new Markup(GameManager.AssetManager.GetString(descriptor(t, m))));

    public RoomBuilder ProcessToData<TContext>(string id, Func<TContext, AssetManager, IRenderable> descriptor)
        => ProcessToData<TContext>(id, (t, m) => new StaticDisplayData(int.MinValue, descriptor(t, m)));

    public RoomBuilder ProcessToData<TContext>(string id, Func<TContext, AssetManager, IDisplayData> descriptor)
    {
        _assetManager.Add(id, CreateContextProcessor(descriptor));

        _roomState.ProcessorNames.Add(id);

        return this;
    }

    public RoomBuilder ProcessToCommand<TContext>(string name, Func<TContext, AssetManager, IGameCommand> commands)
        => ProcessToCommand(name, new Func<TContext, AssetManager, IDisplayCommand>((c, m) => new StaticDiplayCommand(name, int.MinValue, commands(c, m))));

    public RoomBuilder ProcessToCommand<TContext>(string name, Func<TContext, AssetManager, IDisplayCommand> commands)
    {
        var assetId = $"{_roomState.Id}_{name}";

        _assetManager.Add(assetId, CreateContextProcessor(commands));

        _roomState.ProcessorNames.Add(assetId);

        return this;
    }


    private static Func<ContextProcessor> CreateContextProcessor<TContext, TResult>(Func<TContext, AssetManager, TResult> builder)
    {
        IEnumerable<object?> Create(AssetManager assetmanager, object context)
        {
            if(context is TContext typedContext)
                yield return builder(typedContext, assetmanager);
        }

        return () => Create;
    }
}