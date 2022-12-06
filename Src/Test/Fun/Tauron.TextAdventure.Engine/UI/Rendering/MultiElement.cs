using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.TextAdventure.Engine.UI.Rendering;

[PublicAPI]
public sealed class MultiElement : RenderElement
{
    public ImmutableList<RenderElement> Elements { get; private set; }

    public MultiElement()
        => Elements = ImmutableList<RenderElement>.Empty;

    private MultiElement(ImmutableList<RenderElement> elements)
        => Elements = elements;

    public override RenderElement WithTag(in RenderTag tag)
    {
        foreach (RenderElement renderElement in Elements.Where(e => e.Tag.IsEmpty))
            renderElement.WithTag(tag);

        return base.WithTag(tag);
    }

    public static MultiElement Add(MultiElement element, RenderElement toRender) => new(element.Elements.Add(toRender));
    
    public static MultiElement AddRange(MultiElement element, IEnumerable<RenderElement> toRender) => new(element.Elements.AddRange(toRender));

    public static MultiElement Create(params RenderElement[] elements) => new(ImmutableList<RenderElement>.Empty.AddRange(elements));
    public override void Accept(IRenderVisitor visitor)
        => visitor.VisitMulti(this);
}