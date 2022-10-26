using NRules.Fluent;
using SpaceConqueror.Core;
using SpaceConqueror.Modules;
using SpaceConqueror.States;
using Spectre.Console;

namespace SpaceConqueror;

public sealed class GameManager : IAsyncDisposable
{
    public static readonly Version GameVersion = new(0, 1);
    
    private readonly string _modFolder;

    public GlobalState State { get; private set; } = null!;

    public ModuleManager ModuleManager { get; } = new();
    
    public GameManager(string modFolder)
        => _modFolder = modFolder;

    public async Task Initialize(StatusContext context)
    {
        AnsiConsole.WriteLine("Module Laden...");

        RuleRepository rules = new();
        StateRegistrar stateRegistrar = new();
        
        await ModuleManager.LoadModules(_modFolder, new ModuleConfiguration(rules, stateRegistrar), AnsiConsole.WriteLine, e => AnsiConsole.WriteException(e));
        
        AnsiConsole.WriteLine("Finalisiere Daten");
        State = new GlobalState(rules.CompileFast(), stateRegistrar.GetStates());
    }

    public async ValueTask Run()
    {
        var menu = new MainMenu(this);
        await menu.Show();
    }

    public ValueTask DisposeAsync()
        => default;
}