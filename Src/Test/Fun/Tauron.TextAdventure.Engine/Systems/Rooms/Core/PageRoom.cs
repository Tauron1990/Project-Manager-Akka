using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Core;

public sealed class PageRoom : BaseRoom
{
    private readonly Func<RenderElement> _element;

    public PageRoom(Func<RenderElement> element)
        => _element = element;

    protected internal override bool CanReturn { get; }
    
    protected internal override RenderElement CreateRender()
        => throw new NotImplementedException();

    protected internal override IEnumerable<CommandPairBase> CreateCommands()
        => throw new NotImplementedException();
}