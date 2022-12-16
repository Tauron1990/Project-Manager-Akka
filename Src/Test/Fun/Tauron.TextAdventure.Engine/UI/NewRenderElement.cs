using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class NewRenderElement : GameStateAdder
{
    private readonly string _name;
    private readonly RenderElement _element;

    public NewRenderElement(string name, RenderElement element)
    {
        if(element is CommandBase)
            throw new InvalidOperationException("Command Should be add with NewCommand Class");

        _name = name;
        _element = element;
    }

    protected internal override void Apply(GameState gameState)
        => gameState.Get<RenderState>().ToRender.Set(_name, _element);
}