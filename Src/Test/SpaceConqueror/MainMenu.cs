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
                   .UseConverter(c => c switch
                    {
                        MainMenuCommand.NewGame => "Neues Spiel",
                        MainMenuCommand.LoadGame => "Spiel Laden",
                        MainMenuCommand.Modules => "Module",
                        MainMenuCommand.Exit => "Beenden",
                        _ => "Unbekand"
                    })
                   .AddChoices(MainMenuCommand.Modules, MainMenuCommand.Exit)
            );

            switch (command)
            {

                case MainMenuCommand.Modules:
                    await new ModuleMenu(_gameManager.ModuleManager).Show();
                    break;
                case MainMenuCommand.Exit:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}