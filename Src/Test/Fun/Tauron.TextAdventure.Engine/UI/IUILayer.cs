using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

public interface IUILayer
{
    IRenderVisitor CreateForPage();

    ValueTask<string> ExecutePage(IRenderVisitor visitor);

    RenderElement CreateTitle();
}