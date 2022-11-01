namespace SpaceConqueror.States.Rendering;

public sealed class StaticDiplayCommand : DisplayCommandBase
{
    private readonly IGameCommand _gameCommand;

    public StaticDiplayCommand(string name, int order, IGameCommand gameCommand)
        : base(order, name)
        => _gameCommand = gameCommand;

    protected override IEnumerable<IGameCommand> CommandBuilder()
    {
        yield return _gameCommand;
    }
}