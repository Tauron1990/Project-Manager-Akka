using Tauron.TextAdventure.Engine.GamePackages.Core;
using Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class RoomElement<TBuilder> : PackageElement
    where TBuilder : RoomBuilderBase, new()
{
    private readonly string _name;
    private readonly Action<TBuilder> _builderAction;

    public RoomElement(string name, Action<TBuilder> builderAction)
    {
        _name = name;
        _builderAction = builderAction;

    }
    
    internal override void Load(ElementLoadContext context)
    {
        context.Rooms.Add(_name,
            () =>
            {
                var builder = new TBuilder();
                _builderAction(builder);

                return builder;
            });
    }
}