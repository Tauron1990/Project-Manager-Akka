namespace SpaceConqueror.States.Rendering;

public abstract class DisplayCommandBase : IDisplayCommand
{
    protected DisplayCommandBase(int order, string name)
    {
        Order = order;
        Name = name;
        Commands = CommandBuilder;
    }

    public int Order { get; }

    public string Name { get; }

    public Func<IEnumerable<IGameCommand>> Commands { get; }

    protected abstract IEnumerable<IGameCommand> CommandBuilder();
}