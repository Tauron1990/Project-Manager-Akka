using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Console;

public abstract class SpectreConsoleLayerBase<TVisitor> : IUILayer
    where TVisitor : SpectreVisitor
{
    public IRenderVisitor CreateForPage()
        => CreateForConsole();

    protected abstract TVisitor CreateForConsole();

    public ValueTask<string> ExecutePage(IRenderVisitor visitor)
    {
        if(visitor is TVisitor spectreVisitor)
            return ExecutePage(spectreVisitor);

        throw new InvalidOperationException("The render Visitor is not Compatible with this UI Layer");
    }

    public abstract RenderElement CreateTitle();

    protected abstract ValueTask<string> ExecutePage(TVisitor visitor);
}