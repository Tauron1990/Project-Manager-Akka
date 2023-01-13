using Tauron.TextAdventure.Engine.Data;

namespace Tauron.TextAdventure.Engine.Systems.Actor;

public interface IActor : IState
{
    string Id { get; }

    ReactiveProperty<string> Location { get; }
}