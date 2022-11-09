using SpaceConqueror.States;
using Spectre.Console;

namespace SpaceConqueror.SubMenus;

public class LoadGamesMenu
{
    private readonly GlobalState _globalState;

    public LoadGamesMenu(GlobalState globalState)
        => _globalState = globalState;

    public ValueTask<bool> Show()
    {
        AnsiConsole.Clear();

        var entries = GlobalState.GetSaveGames()
           .Select(s => new LoadEntry(false, s))
           .ToList();

        entries.Add(new LoadEntry(true, "Zurück"));

        LoadEntry result = AnsiConsole.Prompt(
            new SelectionPrompt<LoadEntry>()
               .AddChoices(entries)
               .UseConverter(e => e.Name)
               .Title("Spiel Laden")
               .HighlightStyle(Styles.SelectionColor));

        if(result.IsExit) return ValueTask.FromResult(false);

        _globalState.LoadGame(result.Name);

        return ValueTask.FromResult(true);
    }

    private sealed record LoadEntry(bool IsExit, string Name);
}