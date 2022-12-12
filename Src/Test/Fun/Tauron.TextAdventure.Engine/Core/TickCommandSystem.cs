using System.Reactive.Linq;
using Tauron.TextAdventure.Engine.Systems;

namespace Tauron.TextAdventure.Engine.Core;

public sealed class TickCommandSystem : ISystem
{
    public IEnumerable<IDisposable> Initialize(EventManager eventManager)
    {
        yield return eventManager.OnCommand<TickCommand>().SelectMany(tc => tc.Commands).Subscribe(eventManager.SendCommand<IGameCommand>());
    }
}