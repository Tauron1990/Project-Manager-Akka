namespace Tauron.TextAdventure.Engine.UI.Rendering;

public abstract class RenderElement
{
    public RenderTag Tag { get; set; }

    public RenderElement WithTag(RenderTag tag)
    {
        Tag = tag;

        return this;
    }
    
    public abstract void Accept(IRenderVisitor visitor);
}