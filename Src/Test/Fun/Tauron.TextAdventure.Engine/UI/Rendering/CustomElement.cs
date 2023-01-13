namespace Tauron.TextAdventure.Engine.UI.Rendering;

public abstract class CustomElement : RenderElement
{
    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitCustom(this);
}