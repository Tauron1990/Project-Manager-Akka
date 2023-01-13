using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

public interface IRenderVisitor
{
    void Visit(RenderElement element);
    void VisitCustom(CustomElement customElement);
    void VisitGameTitle(GameTitleElement gameTitleElement);
    void VisitMulti(MultiElement multiElement);
    void VisitCommandMenu(CommandMenu commandMenu);
    void VisitCommandItem(CommandItem commandItem);
    void VisitSpacing(SpacingElement spacingElement);
    void VisitAsk(AskElement askElement);
    void VisitText(TextElement textElement);
    void VisitDocument(DocumentElement documentElement);
}