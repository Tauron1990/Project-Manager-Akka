using JetBrains.Annotations;
using SpaceConqueror.Core;
using SpaceConqueror.States;
using SpaceConqueror.States.Rooms;

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
        => (Rules.AsEnumerable(), States.GetStates());

    public RoomBuilder ExcludeFromHistory()
    {
        _roomState.ExcludeHistory = true;

        return this;
    }
}