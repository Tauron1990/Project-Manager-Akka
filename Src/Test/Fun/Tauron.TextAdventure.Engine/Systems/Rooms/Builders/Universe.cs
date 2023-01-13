using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.GamePackages;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

[PublicAPI]
public class Universe
{
    private bool _modify;

    private List<PackageElement> _rooms = new();

    public Universe(bool modify)
        => _modify = modify;

    public static PackageElement Create(Action<Universe> factory)
    {
        var uni = new Universe(false);
        factory(uni);

        return PackageElement.Group(uni._rooms);
    }

    public static PackageElement Modify(Action<Universe> factory)
    {
        var uni = new Universe(true);
        factory(uni);

        return PackageElement.Group(uni._rooms);
    }

    private Universe WithBuilder<TBuilder>(string name, Action<TBuilder> builder)
        where TBuilder : RoomBuilderBase, new()
    {
        _rooms.Add(
            _modify
                ? PackageElement.ModifyRoom(name, builder)
                : PackageElement.NewRoom(name, builder));

        return this;
    }

    public Universe WithAsk(string name, Action<AskBuilder> builder)
        => WithBuilder(name, builder);

    public Universe WithCustom(string name, Action<CustomBuilder> builder)
        => WithBuilder(name, builder);

    public Universe WithPage(string name, Action<PageBuilder> builder)
        => WithBuilder(name, builder);
}