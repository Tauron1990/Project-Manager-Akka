using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.Systems;

[PublicAPI]
public interface ISystem
{
    IEnumerable<IDisposable> Initialize(EventManager eventManager);
}