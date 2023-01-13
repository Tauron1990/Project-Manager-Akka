using Spectre.Console.Rendering;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Console.Render;

public sealed class RenderableElement : CustomElement
{
    public RenderableElement(IRenderable toRender)
        => ToRender = toRender;

    public IRenderable ToRender { get; }
}