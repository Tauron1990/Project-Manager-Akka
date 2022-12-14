using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Systems.Rooms;

public abstract class BaseRoom
{
    protected internal abstract bool CanReturn { get; }
    
    protected internal abstract RenderElement CreateRender();

    protected internal abstract IEnumerable<CommandPairBase> CreateCommands();
}