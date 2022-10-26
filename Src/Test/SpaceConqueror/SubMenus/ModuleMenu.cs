using SpaceConqueror.Modules;
using Spectre.Console;

namespace SpaceConqueror.SubMenus;

public sealed class ModuleMenu
{
    private readonly ModuleManager _moduleManager;

    public ModuleMenu(ModuleManager moduleManager)
        => _moduleManager = moduleManager;

    public async ValueTask Show()
    {
        AnsiConsole.Clear();

        await Task.Delay(100);
        
        AnsiConsole.Write(new Rule("Geladene Module").Alignment(Justify.Center));
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        foreach (var gamePackage in _moduleManager.Packages)
        {
            Table modules = new Table()
               .AddColumns(new TableColumn("Name").Alignment(Justify.Left), new TableColumn("Version").Alignment(Justify.Right))
               .Alignment(Justify.Center);

            foreach (RegistratedModule module in gamePackage.Value.Modules)
                modules.AddRow(module.Name, module.Version.ToString());
            
            AnsiConsole.Write(new Panel(modules)
               .Expand()
               .Header($"Mod: {gamePackage.Value.Name}", Justify.Center));
        }

        Console.ReadKey();
    }
}