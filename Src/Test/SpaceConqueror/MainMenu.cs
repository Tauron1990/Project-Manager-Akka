using SpaceConqueror.SubMenus;
using Spectre.Console;

namespace SpaceConqueror;

public sealed class MainMenu
{
    private readonly GameManager _gameManager;

    public MainMenu(GameManager gameManager)
        => _gameManager = gameManager;

    public async ValueTask Show()
    {
        while (true)
        {
            AnsiConsole.Clear();
            await Task.Delay(100);

            AnsiConsole.Write(new Rule("Main Menu").Alignment(Justify.Center));
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            MainMenuCommand command = AnsiConsole.Prompt
            (
                new SelectionPrompt<MainMenuCommand>()
                   .HighlightStyle(Styles.SelectionColor)
                   .UseConverter(
                        c => c switch
                        {
                            MainMenuCommand.NewGame => "Neues Spiel",
                            MainMenuCommand.LoadGame => "Spiel Laden",
                            MainMenuCommand.Modules => "Module",
                            MainMenuCommand.Exit => "Beenden",
                            _ => "Unbekand"
                        })
                   .AddChoices(MainMenuCommand.NewGame, MainMenuCommand.LoadGame, MainMenuCommand.Modules, MainMenuCommand.Exit)
            );

            try
            {
                switch (command)
                {

                    case MainMenuCommand.Modules:
                        await new ModuleMenu(_gameManager.ModuleManager).Show();

                        break;
                    case MainMenuCommand.Exit:
                        return;
                    case MainMenuCommand.NewGame:
                        _gameManager.State.NewGame();
                        await new GameMenu(_gameManager).RunGame();

                        break;
                    case MainMenuCommand.LoadGame:
                        if(await new LoadGamesMenu(_gameManager.State).Show())
                            await new GameMenu(_gameManager).RunGame();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine("Schwerer Fehler");
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenPaths);
                Console.ReadKey();
            }
        }
    }
}