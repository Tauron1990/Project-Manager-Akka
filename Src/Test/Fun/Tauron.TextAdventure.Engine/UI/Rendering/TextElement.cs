namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class TextElement : RenderElement
{
    public string Test { get; }

    public TextElement(string test)
        => Test = test;
    
    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitText(this);
}