using Tauron.TextAdventure.Engine.GamePackages;
using Tauron.TextAdventure.Engine.Systems;
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
                uni.WithPage(
                    RoomKeys.Start,
                    builder =>
                    {
                        builder
                           .WithContent(man => man.GetDocument("Intro1"))
                           .WithCommand(() => new MoveToRommCommand(RoomKeys.End));
                    });

                uni.WithPage(
                    RoomKeys.End,
                    builder =>
                    {
                        builder
                           .WithContent(man => man.GetDocument("Credits"))
                           .WithCommand(() => new EndGameCommand());
                    });
            });
    }
}