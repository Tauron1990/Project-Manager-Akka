using Tauron.TextAdventure.Engine;
using Tauron.TextAdventure.Engine.Console;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace SpaceConqueror.UI;

public sealed class ConsoleUI : SpectreConsoleLayerBase<ConsoleUIVisitor>
{
    private readonly AssetManager _manager;

    public ConsoleUI(AssetManager manager, GameBase game)
        : base(game)
        => _manager = manager;

    protected override ConsoleUIVisitor CreateForConsole() 
        => new();

    public override RenderElement CreateTitle()
        => new GameTitleElement();

    protected override async ValueTask<string?> ExecutePage(ConsoleUIVisitor visitor)
    {
        if(visitor.RootInputElement is null)
        {
            visitor.RunRender();

            return string.Empty;
        }
        else
            return await visitor.RootInputElement.Execute(_manager, visitor.RunRender).ConfigureAwait(false);
    }
}