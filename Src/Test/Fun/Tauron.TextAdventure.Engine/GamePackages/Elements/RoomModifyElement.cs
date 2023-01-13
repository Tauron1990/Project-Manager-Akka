using Tauron.TextAdventure.Engine.GamePackages.Core;
using Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class RoomModifyElement<TBuilder> : PackageElement
    where TBuilder : RoomBuilderBase, new()
{
    private readonly Action<TBuilder> _builderAction;
    private readonly string _name;

    public RoomModifyElement(string name, Action<TBuilder> builderAction)
    {
        _name = name;
        _builderAction = builderAction;

    }

    internal override void Load(ElementLoadContext context)
    {
        context.RoomModify.Add(_name, b => _builderAction((TBuilder)b));
    }
}