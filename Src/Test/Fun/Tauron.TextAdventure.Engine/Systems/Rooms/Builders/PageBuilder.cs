using Cottle;
using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.Systems.Rooms.Core;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Builders;

[PublicAPI]
public sealed class PageBuilder : RoomBuilderBase
{
    private Func<IGameCommand>? _next;
    private string _nextLabel = UiKeys.Room.Next;
    private Func<AssetManager, RenderElement>? _renderElement;

    public PageBuilder WithLabel(string label)
    {
        _nextLabel = label;

        return this;
    }

    public PageBuilder WithCommand(Func<IGameCommand> command)
    {
        _next = _next.Combine(command);

        return this;
    }

    public PageBuilder WithContent(Func<AssetManager, string> content)
    {
        _renderElement = FromString(content);

        return this;
    }

    public PageBuilder WithContent(Func<AssetManager, IDocument> content)
    {
        _renderElement = FromDocument(content);

        return this;
    }

    public PageBuilder WithContent(Func<AssetManager, RenderElement> content)
    {
        _renderElement = content;

        return this;
    }

    private static Func<AssetManager, RenderElement> FromDocument(Func<AssetManager, IDocument> builder)
        => manager => new DocumentElement(builder(manager), manager.RenderContext);

    private static Func<AssetManager, RenderElement> FromString(Func<AssetManager, string> builder)
        => manager => new TextElement(builder(manager));

    protected internal override BaseRoom CreateRoom(AssetManager assetManager)
    {
        if(_next is null)
            throw new InvalidOperationException("No Next Command Provided");

        if(_renderElement is null)
            throw new InvalidOperationException("No Page Content Provided");

        return new PageRoom(GetRendereElementFactory(assetManager, _renderElement), _nextLabel, _next);
    }

    private static Func<RenderElement> GetRendereElementFactory(AssetManager manager, Func<AssetManager, RenderElement> factory)
        => () => factory(manager);
}