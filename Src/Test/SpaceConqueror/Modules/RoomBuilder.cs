using SpaceConqueror.States;
using SpaceConqueror.States.Rooms;

namespace SpaceConqueror.Modules;

public class RoomBuilder
{
    internal List<Type> Rules { get; } = new();
    internal StateRegistrar States { get; } = new();

    private readonly RoomState _roomState;

    internal RoomBuilder(string name)
        => _roomState = new RoomState(name);

    internal (IEnumerable<Type> Rules, Func<IEnumerable<IState>> States) Init()
        => (Rules.AsEnumerable(), States.GetStates());
}