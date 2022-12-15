using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.Systems.Rooms.Core;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

public sealed class PageBuilder : RoomBuilderBase
{
    private string _nextLabel = UiKeys.Room.Next;
    private Func<IGameCommand>? _next;
    private Func<RenderElement>? _renderElement;

    protected internal override BaseRoom CreateRoom(AssetManager assetManager)
    {
        if(_next is null)
            throw new InvalidOperationException("No Next Command Provided");

        if(_renderElement is null)
            throw new InvalidOperationException("No Page Content Provided");

        return new PageRoom(_renderElement, _nextLabel, _next);
    }
}