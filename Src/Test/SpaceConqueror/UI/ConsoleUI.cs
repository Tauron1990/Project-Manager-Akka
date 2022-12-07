using Spectre.Console;
using Tauron.TextAdventure.Engine.Console;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.UI;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace SpaceConqueror.UI;

public sealed class ConsoleUI : SpectreConsoleLayerBase<ConsoleUIVisitor>
{
    private readonly AssetManager _manager;

    public ConsoleUI(AssetManager manager)
        => _manager = manager;

    protected override ConsoleUIVisitor CreateForConsole() 
        => new();

    public override RenderElement CreateTitle()
        => new GameTitleElement();

    protected override ValueTask<string> ExecutePage(ConsoleUIVisitor visitor)
        => ExecuteFrame(visitor);

    private async ValueTask<string> ExecuteFrame(ConsoleUIVisitor visitor)
    {
        CommandFrame frame = visitor.Root;
        
        while (true)
        {
            visitor.RunRender();

            var selector = new SelectionPrompt<FrameItem>().Title(frame.Name)
               .AddChoices(frame.CreateItems())
               .PageSize(10)
               .MoreChoicesText(_manager.GetString(UiKeys.More))
               .UseConverter(f => _manager.GetString(f.Label));

            FrameItem result = await selector.ShowAsync(AnsiConsole.Console, default).ConfigureAwait(false);

            if(!result.SubMenu)
                return result.Id;

            frame = frame.GetSubMenu(result.Id);
        }
    }
}