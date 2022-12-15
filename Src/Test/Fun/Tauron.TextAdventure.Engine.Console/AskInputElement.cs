using Cottle;
using Spectre.Console;
using Tauron.TextAdventure.Engine.Core;

namespace Tauron.TextAdventure.Engine.Console;

public sealed class AskInputElement : IInputElement
{
    private readonly string _label;

    public AskInputElement(string label)
        => _label = label;

    public async ValueTask<string> Execute(AssetManager manager, Action reRender)
    {
        reRender();
        
        var textPrompt = new TextPrompt<string>(manager.GetString(_label, Context.Empty));

        return await textPrompt.ShowAsync(AnsiConsole.Console, default).ConfigureAwait(false);
    }
}