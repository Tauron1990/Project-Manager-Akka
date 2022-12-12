using JetBrains.Annotations;
using Tauron.TextAdventure.Engine.Data;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace Tauron.TextAdventure.Engine.UI;

[PublicAPI]
public sealed class NewRenderElement : GameStateAdder
{
    private readonly RenderElement _element;

    public NewRenderElement(RenderElement element)
    {
        if(element is CommandBase)
            throw new InvalidOperationException("Command Should be add with NewCommand Class");

        _element = element;
    }

    protected internal override void Apply(GameState gameState)
        => gameState.Get<RenderState>().ToRender.Append(_element);
}

[PublicAPI]
public sealed class NewCommand : GameStateAdder
{
    private readonly CommandPairBase _command;

    public NewCommand(CommandPairBase command)
        => _command = command;

    protected internal override void Apply(GameState gameState)
        => gameState.Get<RenderState>().Commands.Append(_command);
}