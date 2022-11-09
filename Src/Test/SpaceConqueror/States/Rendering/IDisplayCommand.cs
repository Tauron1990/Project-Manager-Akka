namespace SpaceConqueror.States.Rendering;

public interface IDisplayCommand
{
    int Order { get; }

    string Name { get; }

    Func<IEnumerable<IGameCommand>> Commands { get; }
}