namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class TextElement : RenderElement
{
    public TextElement(string text)
        => Text = text;

    public string Text { get; }

    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitText(this);
}