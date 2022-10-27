using System.Collections.Immutable;
using NRules;
using SpaceConqueror.States;
using SpaceConqueror.States.Rendering;
using Spectre.Console;

namespace SpaceConqueror.SubMenus;

public sealed class GameMenu
{
    private readonly GameManager _manager;
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
            
            Step(session);
            
            foreach (IDisplayData displayData in session.Query<IDisplayData>().OrderBy(dd => dd.Order))
            {
                AnsiConsole.Write(displayData.ToRender);
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule().RoundedBorder());
            }

            List<CommandEntry> commandEntries = new() { new CommandEntry("Menü", () => ShowMenu(session)) };

            commandEntries.AddRange(session
               .Query<IDisplayCommand>()
               .OrderBy(dc => dc.Order)
               .Select(command => new CommandEntry(command.Name, () => session.InsertAll(command.Commands()))));

            commandEntries.Add(new CommandEntry("Beenden", CancelGame));


            ShowPrompt(commandEntries, "Kommandos");
        }
    }

    private void ShowMenu(ISession session)
    {
        List<CommandEntry> menuCommands = new() { new CommandEntry("Haupt Menu", ShowMainMenu) };

        foreach (IMenuItem menuItem in session.Query<IMenuItem>())
            menuCommands.Add(new CommandEntry(menuItem.Name, () => session.InsertAll(menuItem.Commands())));

        menuCommands.Add(new CommandEntry("Zurück", static () => {}));
        
        ShowPrompt(menuCommands, "Menü");
    }

    private void ShowMainMenu()
    {
        List<CommandEntry> menuCommands = new()
                                          {
                                              new CommandEntry("Speichern", SaveGame),
                                              new CommandEntry("Beenden", CancelGame),
                                              new CommandEntry("Zurück", static () => {})
                                          };

        ShowPrompt(menuCommands, "Haupt Menü");
    }

    private void SaveGame()
    {
        try
        {
            AnsiConsole.WriteLine();
            var name = AnsiConsole.Ask<string>("Name des Speicherplatzes?");
            
            _manager.State.SaveGame(name);
        }
        catch (Exception e)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("Fehler beim Speichern");
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenPaths);
            Console.ReadKey();
        }
    }

    private void ShowPrompt(IEnumerable<CommandEntry> commands, string title)
        => AnsiConsole.Prompt(
            new SelectionPrompt<CommandEntry>()
               .Title(title)
               .AddChoices(commands)
               .UseConverter(c => c.Name))
           .Run();

    private void CancelGame()
    {
        if(AnsiConsole.Confirm("Wirklich Beenden", false))
            _run = false;
    }

    private static void Step(ISession session)
    {
        session.Fire();
        var commands = session.Query<IGameCommand>().ToImmutableArray();
        session.RetractAll(commands);
    }

    private sealed record CommandEntry(string Name, Action Run);
}