using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.Systems.Actor;
using Tauron.TextAdventure.Engine.Systems.Rooms;
using Tauron.TextAdventure.Engine.Systems.Rooms.Core;

namespace Tauron.TextAdventure.Engine.Core;

internal static class CorePack
{
    internal static IEnumerable<PackageElement> LoadCore()
    {
        yield return PackageElement.System<RoomManager>();
        yield return PackageElement.System<GameStateAdderSystem>();
        yield return PackageElement.System<TickCommandSystem>();
    }
}