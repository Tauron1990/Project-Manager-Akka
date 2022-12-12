using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.Core;

public sealed class TickCommand : IGameCommand
{
    public IGameCommand[] Commands { get; }

    public TickCommand(IGameCommand[] commands)
        => Commands = commands;
}