using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.Systems;

[PublicAPI]
public interface ISystem
{
    IObservable<object> Initialize(EventManager eventManager);
}