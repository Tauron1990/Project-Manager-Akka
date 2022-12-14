using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Systems.Rooms.Core;

public sealed class AskRoom : BaseRoom
{
    private readonly RenderElement _description;
    private readonly string _label;
    private readonly Func<string, IEnumerable<IGameCommand>> _whenEntered;

    public AskRoom(RenderElement description, string label, Func<string, IEnumerable<IGameCommand>> whenEntered)
    {
        _description = description;
        _label = label;
        _whenEntered = whenEntered;

    }

    protected internal override bool CanReturn => false;
    
    protected internal override RenderElement CreateRender()
        => _description;

    protected internal override IEnumerable<CommandPairBase> CreateCommands()
    {
        yield return new AskCommand(_label, _whenEntered);
    }
}