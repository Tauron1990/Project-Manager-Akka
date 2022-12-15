using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Core;

public sealed class PageRoom : BaseRoom
{
    private readonly Func<RenderElement> _element;
    private readonly string _nextLabel;
    private readonly Func<IGameCommand> _next;

    public PageRoom(Func<RenderElement> element, string nextLabel, Func<IGameCommand> next)
    {
        _element = element;
        _nextLabel = nextLabel;
        _next = next;
    }

    protected internal override RenderElement CreateRender()
        => _element();

    protected internal override IEnumerable<CommandPairBase> CreateCommands()
    {
        yield return new SingleCommand("Next", _nextLabel, () => new SingleElementList<IGameCommand>(_next()));
    }
}