using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.Core;

public sealed class TickCommand : IGameCommand
{
    public TickCommand(IGameCommand[] commands)
        => Commands = commands;

    public IGameCommand[] Commands { get; }
}