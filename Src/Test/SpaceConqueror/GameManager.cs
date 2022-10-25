using NRules;
using NRules.Fluent;
using SpaceConqueror.Modules;
using Spectre.Console;

namespace SpaceConqueror;

public sealed class GameManager : IAsyncDisposable
{
    public static readonly Version GameVersion = new(0, 1);
    
    private readonly string _modFolder;

    public ModuleManager ModuleManager { get; } = new();
    public ISessionFactory SessionFactory { get; private set; } = null!;
    
    public GameManager(string modFolder)
        => _modFolder = modFolder;

    public async Task Initialize(StatusContext context)
    {
        AnsiConsole.WriteLine("Module Laden...");

        var rules = new RuleRepository();
        
        await ModuleManager.LoadModules(_modFolder, new ModuleConfiguration(rules), AnsiConsole.WriteLine);
        
        AnsiConsole.WriteLine("Finalisiere Daten");
        SessionFactory = rules.Compile();
    }

    public async ValueTask Run()
    {
        AnsiConsole.Ask("Test", string.Empty);
    }

    public ValueTask DisposeAsync()
        => default;
}