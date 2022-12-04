using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

public interface IRenderVisitor
{
    void Visit(RenderElement element);
    void VisitCustom(CustomElement customElement);
    void VisitGameTitle(GameTitleElement gameTitleElement);
    void VisitMulti(MultiElement multiElement);
}