using Tauron.TextAdventure.Engine.Systems;
using Tauron.TextAdventure.Engine.UI;

namespace Tauron.TextAdventure.Engine.Core;

public sealed class GameStateAdderSystem : ISystem
{
    public IEnumerable<IDisposable> Initialize(EventManager eventManager)
    {
        yield return eventManager.OnCommand<GameStateAdder>().Subscribe(add => add.Apply(eventManager.GameState));
    }
}