using System.Collections.Immutable;
using JetBrains.Annotations;

namespace SpaceConqueror.States;

public sealed record CommandProcessorState(ImmutableList<IGameCommand> Commands, bool Run) : IState
{
    [UsedImplicitly]
    public CommandProcessorState()
        : this(ImmutableList<IGameCommand>.Empty, false){}
}