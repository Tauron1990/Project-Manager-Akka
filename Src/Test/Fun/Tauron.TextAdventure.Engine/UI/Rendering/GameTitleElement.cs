namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class GameTitleElement : RenderElement
{
    public override void Accept(IRenderVisitor visitor)
    {
        visitor.VisitGameTitle(this);
    }
}