namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class SpacingElement : RenderElement
{
    public int Amount { get; init; }

    public override void Accept(IRenderVisitor visitor)
    {
        visitor.VisitSpacing(this);
    }
}