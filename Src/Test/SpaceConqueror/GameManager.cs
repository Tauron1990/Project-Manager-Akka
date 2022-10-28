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
        RuleRegistrar ruleRegistrar = new();
        ManagerRegistrar managerRegistrar = new(stateRegistrar);
        RoomRegistrar roomRegistrar = new();
        
        await ModuleManager.LoadModules(
            _modFolder,
            new ModuleConfiguration(
                rules,
                stateRegistrar,
                ruleRegistrar,
                managerRegistrar,
                roomRegistrar),
            AnsiConsole.WriteLine,
            e => AnsiConsole.WriteException(e));

        List<Func<IEnumerable<IState>>> states = new List<Func<IEnumerable<IState>>>
                                                  {
                                                      stateRegistrar.GetStates()
                                                  };

        foreach (var rule in roomRegistrar.GetRules())
        {
            rules.Load(rs => rs.From(rule.Rules));
            states.Add(rule.States);
        }
        
        rules.Load(rs => rs.From(ruleRegistrar.GetRules()));
        rules.Load(rs => rs.From(managerRegistrar.GetRules()));
        
        AnsiConsole.WriteLine("Finalisiere Daten");
        State = new GlobalState(rules.CompileFast(), Wrap(states));

        static Func<IEnumerable<IState>> Wrap(IEnumerable<Func<IEnumerable<IState>>> states)
            => () => states.SelectMany(f => f());
    }

    public async ValueTask Run()
    {
        var menu = new MainMenu(this);
        await menu.Show();
    }

    public ValueTask DisposeAsync()
        => default;
}