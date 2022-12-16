namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class TextElement : RenderElement
{
    public string Text { get; }

    public TextElement(string text)
        => Text = text;
    
    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitText(this);
}