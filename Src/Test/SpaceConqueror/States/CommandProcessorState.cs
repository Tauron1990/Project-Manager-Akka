namespace SpaceConqueror.States;

public sealed class CommandProcessorState : IState
{
    public List<ICommand> Commands { get; set; } = new();
}