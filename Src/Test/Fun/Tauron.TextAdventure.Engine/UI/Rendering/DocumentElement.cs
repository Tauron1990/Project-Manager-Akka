using Cottle;

namespace Tauron.TextAdventure.Engine.UI.Rendering;

public sealed class DocumentElement : RenderElement
{
    public IContext Context { get; set; } = Cottle.Context.Empty;

    public IDocument Document { get; set; } = Cottle.Document.Empty;
    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitDocument(this);
}