using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.UI;

public abstract class GameStateAdder : IGameCommand
{
    protected internal abstract void Apply(GameState gameState);
}