namespace SpaceConqueror.States.Rendering;

public interface IMenuItem
{
    int Order { get; }

    string Name { get; }

    Func<IEnumerable<IGameCommand>> Commands { get; }
}