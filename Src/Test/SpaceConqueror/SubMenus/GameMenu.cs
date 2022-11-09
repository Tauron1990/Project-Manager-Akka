using System.Collections.Immutable;
using NRules;
using SpaceConqueror.Core;
using SpaceConqueror.States;
using SpaceConqueror.States.Rendering;
using Spectre.Console;

namespace SpaceConqueror.SubMenus;

public sealed class GameMenu
{
    private readonly GameManager _manager;
    private readonly List<object> _toRemove = new();
    private bool _run = true;

    public GameMenu(GameManager manager)
        => _manager = manager;

    public async ValueTask RunGame()
    {
        await Task.Delay(100);

        ISession session = _manager.State.Updater;

        session.Insert(new InitializeGameCommand());

        while (_run)
        {
            AnsiConsole.Clear();

            session.RetractAll(_toRemove);

            Step(session);

            _toRemove.AddRange(session.Query<IDisplayData>());
            _toRemove.AddRange(session.Query<IDisplayCommand>());

            foreach (IDisplayData displayData in session.Query<IDisplayData>().OrderBy(dd => dd.Order))
            {
                AnsiConsole.Write(displayData.ToRender);
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule().RoundedBorder());
            }

            List<CommandEntry> commandEntries = new() { new CommandEntry(GetString("Menü"), () => ShowMenu(session)) };

            commandEntries.AddRange(
                session
                   .Query<IDisplayCommand>()
                   .OrderBy(dc => dc.Order)
                   .Select(command => new CommandEntry(GetString(command.Name), () => session.InsertAll(command.Commands()))));

            commandEntries.Add(new CommandEntry(GetString("Beenden"), CancelGame));


            ShowPrompt(commandEntries, GetString("Kommandos"));
        }
    }

    private void ShowMenu(ISession session)
    {
        List<CommandEntry> menuCommands = new() { new CommandEntry(GetString("Haupt Menu"), ShowMainMenu) };

        foreach (IMenuItem menuItem in session.Query<IMenuItem>())
            menuCommands.Add(new CommandEntry(GetString(menuItem.Name), () => session.InsertAll(menuItem.Commands())));

        menuCommands.Add(new CommandEntry(GetString("Zurück"), static () => { }));

        ShowPrompt(menuCommands, GetString("Menü"));
    }

    private void ShowMainMenu()
    {
        List<CommandEntry> menuCommands = new()
                                          {
                                              new CommandEntry(GetString("Speichern"), SaveGame),
                                              new CommandEntry(GetString("Beenden"), CancelGame),
                                              new CommandEntry(GetString("Zurück"), static () => { })
                                          };

        ShowPrompt(menuCommands, GetString("Haupt Menü"));
    }

    private void SaveGame()
    {
        try
        {
            AnsiConsole.WriteLine();
            var name = AnsiConsole.Ask<string>(GetString("Name des Speicherplatzes?"));

            _manager.State.SaveGame(name);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine(GetString("Fehler beim Speichern"));
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenPaths);
            Console.ReadKey();
        }
    }

    private void ShowPrompt(IEnumerable<CommandEntry> commands, string title)
        => AnsiConsole.Prompt(
                new SelectionPrompt<CommandEntry>()
                   .Title(title)
                   .HighlightStyle(Styles.SelectionColor)
                   .AddChoices(commands)
                   .UseConverter(c => c.Name))
           .Run();

    private void CancelGame()
    {
        if(AnsiConsole.Confirm(GetString("Wirklich Beenden"), false))
            _run = false;
    }

    private static void Step(ISession session)
    {
        session.Fire();

        var commands = session.Query<IGameCommand>().ToImmutableArray();
        var states = session.Query<CommandProcessorState>()
           .AsEnumerable()
           .Select(
                cps =>
                {
                    cps.Run = true;

                    return cps;
                })
           .ToImmutableArray();

        session.RetractAll(commands);
        session.UpdateAll(states);
    }

    private string GetString(string input)
        => GameManager.AssetManager.GetString(input);

    private sealed record CommandEntry(string Name, Action Run);
}