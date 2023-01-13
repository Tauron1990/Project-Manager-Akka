namespace Tauron.TextAdventure.Engine.UI.Rendering;

public abstract class RenderElement
{
    public RenderTag Tag { get; set; }

    public virtual RenderElement WithTag(in RenderTag tag)
    {
        Tag = tag;

        return this;
    }

    public abstract void Accept(IRenderVisitor visitor);
}