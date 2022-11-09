using System.Collections.Immutable;

namespace SpaceConqueror.States;

public sealed class CommandProcessorState : IState
{
    public ImmutableList<IGameCommand> Commands { get; internal set; } = ImmutableList<IGameCommand>.Empty;

    public bool Run { get; internal set; }
}