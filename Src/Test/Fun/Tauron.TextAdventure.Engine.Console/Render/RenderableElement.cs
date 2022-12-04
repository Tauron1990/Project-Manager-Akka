using Spectre.Console.Rendering;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.Console.Render;

public sealed class RenderableElement : CustomElement
{
    public IRenderable ToRender { get; }

    public RenderableElement(IRenderable toRender)
        => ToRender = toRender;
}