using Cottle;
using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.UI.Rendering;

[PublicAPI]
public sealed class DocumentElement : RenderElement
{
    public DocumentElement() { }

    public DocumentElement(IDocument document, IContext context)
    {
        Document = document;
        Context = context;
    }

    public IContext Context { get; set; } = Cottle.Context.Empty;

    public IDocument Document { get; set; } = Cottle.Document.Empty;

    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitDocument(this);
}