﻿using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class NewCommand : GameStateAdder
{
    private readonly CommandPairBase _command;
    private readonly string _name;

    public NewCommand(string name, CommandPairBase command)
    {
        _name = name;
        _command = command;
    }

    protected internal override void Apply(GameState gameState)
        => gameState.Get<RenderState>().Commands.Set(_name, _command);
}