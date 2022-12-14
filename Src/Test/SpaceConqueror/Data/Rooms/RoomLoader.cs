using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.Systems.Rooms;
using Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

namespace SpaceConqueror.Data.Rooms;

public static class RoomLoader
{
    public static PackageElement Load()
    {
        return Universe.Create(
            uni =>
            {
            });
    }
}